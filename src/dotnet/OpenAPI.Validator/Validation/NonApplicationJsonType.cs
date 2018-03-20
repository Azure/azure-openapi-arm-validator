﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using AutoRest.Core.Logging;
using OpenAPI.Validator.Model;
using OpenAPI.Validator.Properties;
using OpenAPI.Validator.Validation.Core;

namespace OpenAPI.Validator.Validation
{
    public class NonApplicationJsonType : TypedRule<string>
    {
        private const string AppJsonType = "application/json";

        /// <summary>
        /// Id of the Rule.
        /// </summary>
        public override string Id => "R2004";

        /// <summary>
        /// Violation category of the Rule.
        /// </summary>
        public override ValidationCategory ValidationCategory => ValidationCategory.RPCViolation;

        /// <summary>
        /// This rule passes if the entity contains application/json type
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        public override bool IsValid(string entity)
            => (entity.ToLowerInvariant().StartsWith(AppJsonType));

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
        public override string MessageTemplate => Resources.NonAppJsonTypeNotSupported;

        /// <summary>
        /// The severity of this message (ie, debug/info/warning/error/fatal, etc)
        /// </summary>
        public override Category Severity => Category.Warning;

        /// <summary>
        /// What kind of open api document type this rule should be applied to
        /// </summary>
        public override ServiceDefinitionDocumentType ServiceDefinitionDocumentType => ServiceDefinitionDocumentType.ARM;

        /// <summary>
        /// Rule needs to be run for every single json in individual state
        /// </summary>
        public override ServiceDefinitionDocumentState ValidationRuleMergeState => ServiceDefinitionDocumentState.Individual;

    }
}
