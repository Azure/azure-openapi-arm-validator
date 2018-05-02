﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using AutoRest.Core.Logging;
using OpenAPI.Validator.Properties;
using OpenAPI.Validator.Validation.Core;
using OpenAPI.Validator.Model;
using System.Collections.Generic;

namespace OpenAPI.Validator.Validation
{
    /// <summary>
    /// Validates if GUID is used in any of the properties.
    /// GUID usage is not recommended in general.
    /// </summary>
    public class GuidUsage : TypedRule<Dictionary<string, Schema>>
    {
        /// <summary>
        /// Id of the Rule.
        /// </summary>
        public override string Id => "R3017";

        /// <summary>
        /// Violation category of the Rule.
        /// </summary>
        public override ValidationCategory ValidationCategory => ValidationCategory.ARMViolation;

        /// <summary>
        /// The template message for this Rule. 
        /// </summary>
        /// <remarks>
        /// This may contain placeholders '{0}' for parameterized messages.
        /// </remarks>
        public override string MessageTemplate => Resources.GuidUsageNotRecommended;

        /// <summary>
        /// The severity of this message (ie, debug/info/warning/error/fatal, etc)
        /// </summary>
        public override Category Severity => Category.Warning;

        /// <summary>
        /// What kind of open api document type this rule should be applied to
        /// </summary>
        public override ServiceDefinitionDocumentType ServiceDefinitionDocumentType => ServiceDefinitionDocumentType.ARM | ServiceDefinitionDocumentType.DataPlane;

        /// <summary>
        /// The rule could be violated by a porperty of a model referenced by many jsons belonging to the same
        /// composed state, to reduce duplicate messages, run validation rule in composed state
        /// </summary>
        public override ServiceDefinitionDocumentState ValidationRuleMergeState => ServiceDefinitionDocumentState.Composed;

        /// <summary>
        /// An <paramref name="definitions"/> fails this rule if one of the property has GUID,
        /// i.e. if the type of the definition is string and the format is uuid.
        /// </summary>
        /// <param name="definitions">Operation Definitions to validate</param>
        /// <param name="context">The rule context</param>
        /// <returns>list of validation messages</returns>
        public override IEnumerable<ValidationMessage> GetValidationMessages(Dictionary<string, Schema> definitions, RuleContext context)
        {
            foreach (KeyValuePair<string, Schema> definition in definitions)
            {
                object[] formatParameters;
                if (!this.HandleSchema((Schema)definition.Value, definitions, out formatParameters, definition.Key))
                {
                    formatParameters[1] = definition.Key;
                    yield return new ValidationMessage(new FileObjectPath(context.File,
                                context.Path.AppendProperty(definition.Key).AppendProperty("properties").AppendProperty((string)formatParameters[0])), this, formatParameters);
                }
            }
        }

        private bool HandleSchema(Schema definition, Dictionary<string, Schema> definitions, out object[] formatParameters, string name)
        {
            // This could be a reference to another definition. But, that definition could be handled seperately.
            if (definition.Type == DataType.String && definition.Format?.EqualsIgnoreCase("uuid") == true)
            {
                formatParameters = new object[2];
                formatParameters[0] = name;
                return false;
            }

            if (definition.RepresentsCompositeType())
            {
                foreach (KeyValuePair<string, Schema> property in definition.Properties)
                {
                    if (!this.HandleSchema((Schema)property.Value, definitions, out formatParameters, property.Key))
                    {
                        return false;
                    }
                }
            }
            formatParameters = new object[0];
            return true;
        }

        /// <summary>
        /// What kind of change implementing this rule can cause.
        /// </summary>
        public override ValidationChangesImpact ValidationChangesImpact => ValidationChangesImpact.ServiceImpactingChanges;
    }
}
