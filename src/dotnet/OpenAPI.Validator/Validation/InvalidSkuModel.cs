﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using AutoRest.Core.Logging;
using System.Collections.Generic;
using OpenAPI.Validator.Model;
using OpenAPI.Validator.Properties;
using System.Text.RegularExpressions;
using System.Linq;
using OpenAPI.Validator.Model.Utilities;
using OpenAPI.Validator.Validation.Core;

namespace OpenAPI.Validator.Validation
{
    /// <summary>
    /// Validates the SKU Model. A Sku model must have name property. 
    /// It can also have tier, size, family, capacity as optional properties.
    /// </summary>
    public class InvalidSkuModel : TypedRule<Dictionary<string, Schema>>
    {
        private static readonly Regex PropertiesRegEx = new Regex(@"^(NAME|TIER|SIZE|FAMILY|CAPACITY)$", RegexOptions.IgnoreCase);

        /// <summary>
        /// The template message for this Rule. 
        /// </summary>
        /// <remarks>
        /// This may contain placeholders '{0}' for parameterized messages.
        /// </remarks>
        public override string MessageTemplate => Resources.SkuModelIsNotValid;

        /// <summary>
        /// Id of the Rule.
        /// </summary>
        public override string Id => "R2057";

        /// <summary>
        /// What kind of change implementing this rule can cause.
        /// </summary>
        public override ValidationChangesImpact ValidationChangesImpact => ValidationChangesImpact.ServiceImpactingChanges;

        /// <summary>
        /// Violation category of the Rule.
        /// </summary>
        public override ValidationCategory ValidationCategory => ValidationCategory.RPCViolation;

        /// <summary>
        /// The severity of this message (ie, debug/info/warning/error/fatal, etc)
        /// </summary>
        public override Category Severity => Category.Warning;

        /// <summary>
        /// What kind of open api document type this rule should be applied to
        /// </summary>
        public override ServiceDefinitionDocumentType ServiceDefinitionDocumentType => ServiceDefinitionDocumentType.ARM;

        /// <summary>
        /// The rule could be violated by a model referenced by many jsons belonging to the same
        /// composed state, to reduce duplicate messages, run validation rule in composed state
        /// </summary>
        public override ServiceDefinitionDocumentState ValidationRuleMergeState => ServiceDefinitionDocumentState.Composed;

        /// <summary>
        /// Validates Sku Model
        /// </summary>
        /// <param name="definitions">to be validated</param>
        /// <param name="context">the rule context</param>
        /// <returns>list of ValidationMessages</returns>
        public override IEnumerable<ValidationMessage> GetValidationMessages(Dictionary<string, Schema> definitions, RuleContext context)
        {
            var modelsNamedSku = definitions.Where(defPair => defPair.Key.EqualsIgnoreCase("sku"));

            foreach (KeyValuePair<string, Schema> definition in modelsNamedSku)
            {
                Schema schema = definition.Value;
                if (schema.Properties?.All(property => PropertiesRegEx.IsMatch(property.Key)) != true ||
                    (schema.Properties?.Any(property => property.Key.EqualsIgnoreCase("name") &&
                                                        (property.Value.Type == Model.DataType.String ||
                                                            (property.Value.Type == null &&
                                                            ValidationUtilities.IsReferenceOfType(property.Value.Reference, definitions, Model.DataType.String)))) != true))
                {
                    yield return new ValidationMessage(new FileObjectPath(context.File, context.Path.AppendProperty(definition.Key)), this, definition.Key);
                }
            }

        }
    }
}
