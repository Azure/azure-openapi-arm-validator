﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using AutoRest.Core.Logging;
using OpenAPI.Validator.Model;
using OpenAPI.Validator.Properties;
using OpenAPI.Validator.Validation.Core;
using OpenAPI.Validator.Validation.Extensions;
using System.Collections.Generic;

namespace OpenAPI.Validator.Validation
{
    /// <summary>
    /// Validates if there is no request body for the delete operation.
    /// </summary>
    public class DeleteMustNotHaveRequestBody : TypedRule<Dictionary<string, Operation>>
    {
        /// <summary>
        /// Id of the Rule.
        /// </summary>
        public override string Id => "R3013";

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
        public override string MessageTemplate => Resources.DeleteMustNotHaveRequestBody;

        /// <summary>
        /// The severity of this message (ie, debug/info/warning/error/fatal, etc)
        /// </summary>
        public override Category Severity => Category.Error;

        /// <summary>
        /// What kind of open api document type this rule should be applied to
        /// </summary>
        public override ServiceDefinitionDocumentType ServiceDefinitionDocumentType => ServiceDefinitionDocumentType.ARM | ServiceDefinitionDocumentType.DataPlane;

        /// <summary>
        /// The rule runs on each operation in isolation irrespective of the state and can be run in individual state
        /// </summary>
        public override ServiceDefinitionDocumentState ValidationRuleMergeState => ServiceDefinitionDocumentState.Individual;

        /// <summary>
        /// An <paramref name="operationDefinition"/> fails this rule if delete operation has a request body.
        /// </summary>
        /// <param name="operationDefinition">Operation Definition to validate</param>
        /// <returns>true if delete operation does not have a request body. false otherwise.</returns>
        public override IEnumerable<ValidationMessage> GetValidationMessages(Dictionary<string, Operation> operationDefinition, RuleContext context)
        {
            var serviceDefinition = (ServiceDefinition)context.Root;
            foreach (string httpVerb in operationDefinition.Keys)
            {
                if (httpVerb.ToLower().Equals("delete"))
                {
                    Operation operation = operationDefinition.GetValueOrNull(httpVerb);

                    if (operation?.Parameters == null)
                        continue;

                    foreach (SwaggerParameter parameter in operation.Parameters)
                    {
                        if (parameter.In == ParameterLocation.Body)
                        {
                            yield return new ValidationMessage(new FileObjectPath(context.File,
                                    context.Path.AppendProperty(httpVerb).AppendProperty("parameters").AppendIndex(operation.Parameters.IndexOf(parameter))), this, operation.OperationId);
                        }
                        else if (serviceDefinition.Parameters.ContainsKey(parameter.Reference?.StripParameterPath() ?? string.Empty))
                        {
                            if (serviceDefinition.Parameters[parameter.Reference.StripParameterPath()].In == ParameterLocation.Body)
                            {
                                yield return new ValidationMessage(new FileObjectPath(context.File,
                                    context.Path.AppendProperty(httpVerb).AppendProperty("parameters").AppendIndex(operation.Parameters.IndexOf(parameter))), this, operation.OperationId);
                            }
                        }
                    }
                }
            }
        }
    }
}
