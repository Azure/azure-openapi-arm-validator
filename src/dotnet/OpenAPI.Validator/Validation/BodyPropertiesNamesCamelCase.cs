﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Collections.Generic;
using AutoRest.Core.Logging;
using OpenAPI.Validator.Properties;
using OpenAPI.Validator.Validation.Core;
using OpenAPI.Validator.Model;
using OpenAPI.Validator.Model.Utilities;

namespace OpenAPI.Validator.Validation
{
    /// <summary>
    /// Property names must be camelCase style
    /// </summary>
    public class BodyPropertiesNamesCamelCase : TypedRule<Dictionary<string, Operation>>
    {

        /// <summary>
        /// Id of the Rule.
        /// </summary>
        public override string Id => "R3014";

        /// <summary>
        /// Violation category of the Rule.
        /// </summary>
        public override ValidationCategory ValidationCategory => ValidationCategory.ARMViolation;

        /// <summary>
        /// What kind of change implementing this rule can cause.
        /// </summary>
        public override ValidationChangesImpact ValidationChangesImpact => ValidationChangesImpact.ServiceImpactingChanges;

        /// <summary>;
        /// The template message for this Rule. 
        /// </summary>
        /// <remarks>
        /// This may contain placeholders '{0}' for parameterized messages.
        /// </remarks>
        public override string MessageTemplate => Resources.BodyPropertyNameCamelCase;

        /// <summary>
        /// The severity of this message (ie, debug/info/warning/error/fatal, etc)
        /// </summary>
        public override Category Severity => Category.Error;

        /// <summary>
        /// What kind of open api document type this rule should be applied to
        /// </summary>
        public override ServiceDefinitionDocumentType ServiceDefinitionDocumentType => ServiceDefinitionDocumentType.Default;

        /// <summary>
        /// The rule could be violated by a porperty of a model referenced by many jsons belonging to the same
        /// composed state, to reduce duplicate messages, run validation rule in composed state
        /// </summary>
        public override ServiceDefinitionDocumentState ValidationRuleMergeState => ServiceDefinitionDocumentState.Composed;

        /// <summary>
        /// Validates whether property names are camelCase in body parameters.
        /// </summary>
        public override IEnumerable<ValidationMessage> GetValidationMessages(Dictionary<string, Operation> path, RuleContext context)
        {
            foreach (string operation in path.Keys)
            {
                if (path[operation]?.Parameters != null)
                {
                    for (var i = 0; i < path[operation].Parameters.Count; ++i)
                    {
                        if (path[operation].Parameters[i].In == ParameterLocation.Body && path[operation].Parameters[i].Schema?.Properties != null)
                        {
                            foreach (KeyValuePair<string, Schema> prop in path[operation].Parameters[i].Schema?.Properties)
                            {
                                if (!ValidationUtilities.IsODataProperty(prop.Key) && !ValidationUtilities.IsNameCamelCase(prop.Key))
                                {
                                    yield return new ValidationMessage(new FileObjectPath(context.File,
                                        context.Path.AppendProperty(operation).AppendProperty("parameters").AppendIndex(i).AppendProperty("schema").AppendProperty("properties").AppendProperty(prop.Key)),
                                        this, prop.Key, ValidationUtilities.GetCamelCasedSuggestion(prop.Key));
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}