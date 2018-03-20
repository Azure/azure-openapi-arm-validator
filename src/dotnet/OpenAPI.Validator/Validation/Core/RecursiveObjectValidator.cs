﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AutoRest.Core.Utilities.Collections;
using System;
using AutoRest.Core.Logging;
using AutoRest.Core.Utilities;
using OpenAPI.Validator.Model;
using Newtonsoft.Json;

namespace OpenAPI.Validator.Validation.Core
{
    /// <summary>
    ///     A validator that traverses an object graph, applies validation rules, and logs validation messages
    /// </summary>
    public class RecursiveObjectValidator
    {
        private Func<PropertyInfo, string> resolver;

        /// <summary>
        /// Initializes the object validator with a custom <paramref name="resolver"/>
        /// that returns the name for a property when setting the location of messages
        /// </summary>
        /// <param name="resolver">A function that resolves the name of a property</param>
        public RecursiveObjectValidator(Func<PropertyInfo, string> resolver)
        {
            this.resolver = resolver;
        }

        /// <summary>
        /// Recursively validates <paramref name="entity"/> by traversing all of its properties
        /// </summary>
        /// <param name="filePath">uri path to the servicedefinition document <paramref name="entity"/></param>
        /// <param name="entity">The object to validate</param><param name="entity">The object to validate</param>
        /// <param name="metaData">The metadata associated with serviceDefinition to which <paramref name="metaData"/> belongs to</param>
        public IEnumerable<ValidationMessage> GetValidationExceptions(Uri filePath, ServiceDefinition entity, ServiceDefinitionMetadata metadata)
        {
            return RecursiveValidate(entity, ObjectPath.Empty, new RuleContext(entity, filePath), Enumerable.Empty<Rule>(), metadata);
        }

        /// <summary>
        /// Recursively validates <paramref name="entity"/> by traversing all of its properties
        /// </summary>
        /// <param name="entity">The object to validate</param>
        /// <param name="metaData">metaData associated with the serviceDefinition</param>
        public IEnumerable<Rule> GetFilteredRules(IEnumerable<Rule> rules, ServiceDefinitionMetadata metaData)
        {
            // Filter by document type
            // By default select all rules, then add the doc type specific rules
            var serviceDefTypeRules = rules.Where(rule => rule.ServiceDefinitionDocumentType == ServiceDefinitionDocumentType.Default);
            if (metaData.ServiceDefinitionDocumentType != ServiceDefinitionDocumentType.Default)
            {
                serviceDefTypeRules = serviceDefTypeRules.Concat(rules.Where(rule => (rule.ServiceDefinitionDocumentType & metaData.ServiceDefinitionDocumentType) != 0));
            }

            // Filter by the current merge state, and return
            return serviceDefTypeRules.Where(rule => rule.ValidationRuleMergeState == metaData.MergeState);
        }

        /// <summary>
        /// Recursively validates <paramref name="entity"/> by traversing all of its properties
        /// </summary>
        /// <param name="entity">The object to validate</param>
        /// <param name="parentContext">The rule context of the object that <paramref name="entity"/> belongs to</param>
        /// <param name="rules">The set of rules from the parent object to apply to <paramref name="entity"/></param>
        /// <param name="metaData">The metadata associated with serviceDefinition to which <paramref name="entity"/> belongs to</param>
        /// <param name="traverseProperties">Whether or not to traverse this <paramref name="entity"/>'s properties</param>
        /// <returns></returns>
        private IEnumerable<ValidationMessage> RecursiveValidate(object entity, ObjectPath entityPath, RuleContext parentContext, IEnumerable<Rule> rules, ServiceDefinitionMetadata metaData, bool traverseProperties = true)
        {
            var messages = Enumerable.Empty<ValidationMessage>();
            if (entity == null)
            {
                return messages;
            }

            // Ensure that the rules can be re-enumerated without re-evaluating the enumeration.
            var collectionRules = rules.ToList();

            var list = entity as IList;
            var dictionary = entity as IDictionary;
            if (traverseProperties && list != null)
            {
                // Recursively validate each list item and add the 
                // item index to the location of each validation message
                var listMessages = list.SelectMany((item, index)
                    => RecursiveValidate(item, entityPath.AppendIndex(index), parentContext.CreateChild(item, index), collectionRules, metaData));
                messages = messages.Concat(listMessages);
            }

            else if (traverseProperties && dictionary != null)
            {
                // Dictionaries that don't provide any type info cannot be traversed, since it result in infinite iteration
                var shouldTraverseEntries = dictionary.IsTraversableDictionary();

                // Recursively validate each dictionary entry and add the entry 
                // key to the location of each validation message
                var dictMessages = dictionary.SelectMany((key, value)
                    => RecursiveValidate(value, entityPath.AppendProperty((string)key), parentContext.CreateChild(value, (string)key), collectionRules, metaData, shouldTraverseEntries));
                messages = messages.Concat(dictMessages);
            }

            // If this is a class, validate its value and its properties.
            else if (traverseProperties && entity.GetType().IsClass() && entity.GetType() != typeof(string))
            {
                // Validate each property of the object
                var propertyMessages = entity.GetValidatableProperties()
                    .SelectMany(p => ValidateProperty(p, p.GetValue(entity), entityPath.AppendProperty(p.Name), parentContext, metaData));
                messages = messages.Concat(propertyMessages);
            }

            // Validate the value of the object itself
            var valueMessages = ValidateObjectValue(entity, collectionRules, parentContext, metaData);
            return messages.Concat(valueMessages);
        }

        /// <summary>
        /// Validates an object value 
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="collectionRules"></param>
        /// <param name="parentContext"></param>
        /// <param name="metaData">The metadata associated with corresponding serviceDefinition</param>
        /// <returns></returns>
        private IEnumerable<ValidationMessage> ValidateObjectValue(object entity,
            IEnumerable<Rule> collectionRules, RuleContext parentContext,
            ServiceDefinitionMetadata metaData)
        {
            // Get any rules defined for the class of the entity
            var classRules = GetFilteredRules(entity.GetType().GetValidationRules(), metaData);

            // Combine the class rules with any rules that apply to the collection that the entity is part of
            classRules = collectionRules.Concat(classRules);

            // Apply each rule for the entity
            return classRules.SelectMany(rule => rule.GetValidationMessages(entity, parentContext));
        }

        /// <summary>
        /// Validates an object property
        /// </summary>
        /// <param name="prop">The property metadata</param>
        /// <param name="value">The property object to validate</param>
        /// <param name="entityPath">Path to the property object in the serviceDefinition</param>
        /// <param name="metaData">The metadata associated with corresponding serviceDefinition</param>
        /// <returns></returns>
        private IEnumerable<ValidationMessage> ValidateProperty(PropertyInfo prop, object value, ObjectPath entityPath, RuleContext parentContext, ServiceDefinitionMetadata metaData)
        {
            // Uses the property name resolver to get the name to use in the path of messages
            var propName = resolver(prop);
            // Determine if anything about this property indicates that it shouldn't be traversed further 
            var shouldTraverseObject = prop.IsTraversableProperty();
            // Create the context that's available to rules that validate this value
            var ruleContext = prop.GetCustomAttribute<JsonExtensionDataAttribute>(true) == null
                ? parentContext.CreateChild(value, propName)
                : parentContext.CreateChild(value, -1);

            // Get any rules defined on this property and any defined as applying to the collection
            var propertyRules = GetFilteredRules(prop.GetValidationRules(), metaData);
            var collectionRules = GetFilteredRules(prop.GetValidationCollectionRules(), metaData);

            // Validate the value of this property against any rules for it
            var propertyMessages = propertyRules.SelectMany(r => r.GetValidationMessages(value, ruleContext));

            // Recursively validate the property (e.g. its properties or any list/dictionary entries),
            // passing any rules that apply to this collection)
            var childrenMessages = RecursiveValidate(value, entityPath, ruleContext, collectionRules, metaData, shouldTraverseObject);

            return propertyMessages.Concat(childrenMessages);
        }
    }
}