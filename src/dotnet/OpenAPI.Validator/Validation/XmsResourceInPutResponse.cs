﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using AutoRest.Core.Logging;
using OpenAPI.Validator.Properties;
using OpenAPI.Validator.Model;
using OpenAPI.Validator.Model.Utilities;
using OpenAPI.Validator.Validation.Core;
using System.Collections.Generic;
using System.Linq;

namespace OpenAPI.Validator.Validation
{
    public class XmsResourceInPutResponse : TypedRule<Dictionary<string, Dictionary<string, Operation>>>
    {
        /// <summary>
        /// Id of the Rule.
        /// </summary>
        public override string Id => "R2062";

        /// <summary>
        /// Violation category of the Rule.
        /// </summary>
        public override ValidationCategory ValidationCategory => ValidationCategory.RPCViolation;

        /// <summary>
        /// The template message for this Rule. 
        /// </summary>
        /// <remarks>
        /// This may contain placeholders '{0}' for parameterized messages.
        /// </remarks>
        public override string MessageTemplate => Resources.PutOperationResourceResponseValidationMessage;

        /// <summary>
        /// The severity of this message (ie, debug/info/warning/error/fatal, etc)
        /// </summary>
        public override Category Severity => Category.Error;

        /// <summary>
        /// What kind of open api document type this rule should be applied to
        /// </summary>
        public override ServiceDefinitionDocumentType ServiceDefinitionDocumentType => ServiceDefinitionDocumentType.ARM;

        /// <summary>
        /// Whether the rule should be applied to the individual or composed context based on
        /// the corresponding .md file
        /// In most cases this should be composed
        /// This is because validation rules that run in individual mode will end up
        /// throwing multiple validation messages for the same violation if related model/property,etc 
        /// was referenced in multiple files
        /// </summary>
        public override ServiceDefinitionDocumentState ValidationRuleMergeState => ServiceDefinitionDocumentState.Composed;

        // Verifies if a tracked resource has a corresponding PATCH operation
        public override IEnumerable<ValidationMessage> GetValidationMessages(Dictionary<string, Dictionary<string, Operation>> paths, RuleContext context)
        {
            var serviceDefinition = context.Root;
            var ops = ValidationUtilities.GetOperationsByRequestMethod("put", serviceDefinition);
            var putRespModels = ops.Select(op => (op.Responses?.ContainsKey("200") != true) ? null : op.Responses["200"].Schema?.Reference?.StripDefinitionPath());
            putRespModels = putRespModels.Where(modelName => !string.IsNullOrEmpty(modelName));
            var xmsResModels = ValidationUtilities.GetXmsAzureResourceModels(serviceDefinition.Definitions);
            var violatingModels = putRespModels.Where(respModel => !ValidationUtilities.EnumerateModelHierarchy(respModel, serviceDefinition.Definitions).Intersect(xmsResModels).Any());
            foreach (var model in violatingModels)
            {
                var violatingOp = ops.First(op => (op.Responses?.ContainsKey("200") == true) && op.Responses["200"]?.Schema?.Reference?.StripDefinitionPath() == model);
                var violatingPath = ValidationUtilities.GetOperationIdPath(violatingOp.OperationId, paths);
                var violatingOpVerb = ValidationUtilities.GetOperationIdVerb(violatingOp.OperationId, violatingPath);
                yield return new ValidationMessage(new FileObjectPath(context.File, context.Path.AppendProperty(violatingPath.Key).AppendProperty(violatingOpVerb)), this, violatingOp.OperationId, model);
            }
        }
    }
}
