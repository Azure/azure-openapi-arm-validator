﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using AutoRest.Core.Logging;
using OpenAPI.Validator.Properties;
using OpenAPI.Validator.Model.Utilities;
using System.Collections.Generic;
using OpenAPI.Validator.Model;
using System.Linq;
using OpenAPI.Validator.Validation.Core;

namespace OpenAPI.Validator.Validation
{
    public class TrackedResourceGetOperation : TypedRule<Dictionary<string, Schema>>
    {
        /// <summary>
        /// Id of the Rule.
        /// </summary>
        public override string Id => "R3025";

        /// <summary>
        /// Violation category of the Rule.
        /// </summary>
        public override ValidationCategory ValidationCategory => ValidationCategory.ARMViolation;

        /// <summary>
        /// What kind of change implementing this rule can cause.
        /// </summary>
        public override ValidationChangesImpact ValidationChangesImpact => ValidationChangesImpact.ServiceImpactingChanges;

        /// <summary>
        /// The template message for this Rule. 
        /// </summary>
        /// <remarks>
        /// This may contain placeholders '{0}' for parameterized messages.
        /// </remarks>
        public override string MessageTemplate => Resources.TrackedResourceGetOperationMissing;

        /// <summary>
        /// The severity of this message (ie, debug/info/warning/error/fatal, etc)
        /// </summary>
        public override Category Severity => Category.Error;

        /// <summary>
        /// What kind of open api document type this rule should be applied to
        /// </summary>
        public override ServiceDefinitionDocumentType ServiceDefinitionDocumentType => ServiceDefinitionDocumentType.ARM;

        /// <summary>
        /// GET operation could be defined in a json different than the one where it is defined, hence need the composed state
        /// </summary>
        public override ServiceDefinitionDocumentState ValidationRuleMergeState => ServiceDefinitionDocumentState.Composed;

        /// <summary>
        /// Verifies if a tracked resource has a corresponding get operation
        /// </summary>
        /// <param name="definitions"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override IEnumerable<ValidationMessage> GetValidationMessages(Dictionary<string, Schema> definitions, RuleContext context)
        {
            // Retrieve the list of TrackedResources
            IEnumerable<string> trackedResources = context.TrackedResourceModels;

            // Retrieve the list of getOperations
            IEnumerable<Operation> getOperations = ValidationUtilities.GetOperationsByRequestMethod("get", context.Root);

            foreach (string trackedResource in trackedResources)
            {
                // check for 200 status response models since they correspond to a successful get operation
                if (!getOperations.Any(op => op.Responses.ContainsKey("200") && (trackedResource).Equals(op.Responses["200"]?.Schema?.Reference?.StripDefinitionPath())))
                {
                    yield return new ValidationMessage(new FileObjectPath(context.File, context.Path.AppendProperty(trackedResource)), this, trackedResource);
                }
            }
        }
    }
}
