// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Linq;
using Xunit;
using System.Collections.Generic;
using OpenAPI.Validator.Validation.Core;
using AutoRest.Core.Logging;
using OpenAPI.Validator.Model;
using OpenAPI.Validator.Validation;
using OpenAPI.Validator.Validation.Extensions;

namespace OpenAPI.Validator.Tests
{

    [Collection("Validation Tests")]
    public partial class OpenAPIModelerValidationTests
    {
        private static readonly string PathToValidationResources = Path.Combine(AutoRest.Core.Utilities.Extensions.CodeBaseDirectory, "Resource", "OpenAPI", "Validation");
        private IEnumerable<ValidationMessage> ValidateOpenAPISpec(string input, ServiceDefinitionMetadata metadata)
        {
            var validator = new RecursiveObjectValidator(PropertyNameResolver.JsonName);
            var serviceDefinition = SwaggerParser.Parse(input, File.ReadAllText(input));
            return validator.GetValidationExceptions(new Uri(input, UriKind.RelativeOrAbsolute), serviceDefinition, metadata).OfType<ValidationMessage>();
        }

        private static IEnumerable<ValidationMessage> GetValidationMessagesForCategory(IEnumerable<ValidationMessage> messages, Category category) => messages.Where(m => m.Severity == category);

        private IEnumerable<ValidationMessage> GetValidationMessagesForRule<TRule>(string fileName) where TRule : Rule
        {
            var ruleInstance = Activator.CreateInstance<TRule>();
            var messages = this.ValidateOpenAPISpec(Path.Combine(PathToValidationResources, fileName), GetMetadataForRuleTest(ruleInstance));
            return GetValidationMessagesForCategory(messages, ruleInstance.Severity).Where(message => message.Rule.GetType() == typeof(TRule));
        }

        private ServiceDefinitionMetadata GetMetadataForRuleTest(Rule rule) =>
             new ServiceDefinitionMetadata
             {
                 ServiceDefinitionDocumentType = rule.ServiceDefinitionDocumentType,
                 MergeState = rule.ValidationRuleMergeState
             };

        [Fact]
        public void BooleanPropertiesValidation()
        {
            var messages = GetValidationMessagesForRule<EnumInsteadOfBoolean>("boolean-properties.json");
            Assert.Equal(messages.Count(), 5);
        }

        [Fact]
        public void UniqueResourcePathsValidation()
        {
            var messages = GetValidationMessagesForRule<UniqueResourcePaths>("network-interfaces-api.json");
            Assert.Equal(messages.Count(), 1);
        }

        [Fact]
        public void NonAppJsonTypeOperationForConsumes()
        {
            var messages = GetValidationMessagesForRule<NonApplicationJsonType>("non-app-json-operation-consumes.json");
            Assert.Equal(messages.Count(), 1);
        }

        [Fact]
        public void NonAppJsonTypeOperationForProduces()
        {
            var messages = GetValidationMessagesForRule<NonApplicationJsonType>("non-app-json-operation-produces.json");
            Assert.Equal(messages.Count(), 1);
        }

        [Fact]
        public void NonAppJsonTypeServiceDefinitionForProduces()
        {
            var messages = GetValidationMessagesForRule<NonApplicationJsonType>("non-app-json-service-def-produces.json");
            Assert.Equal(messages.Count(), 1);
        }

        [Fact]
        public void NonAppJsonTypeServiceDefinitionForConsumes()
        {
            var messages = GetValidationMessagesForRule<NonApplicationJsonType>("non-app-json-service-def-consumes.json");
            Assert.Equal(messages.Count(), 1);
        }

        [Fact]
        public void ArmResourcePropertiesBagValidation()
        {
            var messages = GetValidationMessagesForRule<ArmResourcePropertiesBag>("arm-resource-properties-bag.json");
            Assert.Equal(messages.Count(), 1);
        }

        [Fact]
        public void ArmResourcePropertiesBagMultipleViolationsValidation()
        {
            var messages = GetValidationMessagesForRule<ArmResourcePropertiesBag>("arm-resource-properties-bag-multiple-violations.json");
            Assert.Equal(messages.Count(), 1);
            Assert.Equal(messages.First().Message.Contains("[name, type]"), true);
        }

        [Fact]
        public void ArmResourcePropertiesBagMultipleLevelViolationsValidation()
        {
            var messages = GetValidationMessagesForRule<ArmResourcePropertiesBag>("arm-resource-properties-bag-multiple-level-violations.json");
            Assert.Equal(messages.Count(), 2);
            Assert.Equal(messages.First().Message.Contains("[name, type]"), true);
            Assert.Equal(messages.Last().Message.Contains("[location, id]"), true);
        }

        [Fact]
        public void ArmResourcePropertiesBagWithReferenceValidation()
        {
            var messages = GetValidationMessagesForRule<ArmResourcePropertiesBag>("arm-resource-properties-bag-with-reference.json");
            Assert.Equal(messages.Count(), 1);
        }

        [Fact]
        public void ArmResourcePropertiesBagWithMultipleLevelReferenceValidation()
        {
            var messages = GetValidationMessagesForRule<ArmResourcePropertiesBag>("arm-resource-properties-bag-with-multiple-level-reference.json");
            Assert.Equal(messages.Count(), 1);
        }

        [Fact]
        public void CollectionObjectsPropertiesNamingValidation()
        {
            var messages = GetValidationMessagesForRule<CollectionObjectPropertiesNaming>("collection-objects-naming.json");
            Assert.Equal(messages.Count(), 2);
        }

        [Fact]
        public void BodyTopLevelPropertiesValidation()
        {
            var messages = GetValidationMessagesForRule<BodyTopLevelProperties>("body-top-level-properties.json");
            Assert.Equal(messages.Count(), 1);
        }

        [Fact]
        public void PropertyNameCasingValidation()
        {
            var messages = GetValidationMessagesForRule<BodyPropertiesNamesCamelCase>("property-names-casing.json");
            Assert.Equal(messages.Count(), 1);
            messages = GetValidationMessagesForRule<DefinitionsPropertiesNamesCamelCase>("property-names-casing.json");
            Assert.Equal(messages.Count(), 2);
        }

        [Fact]
        public void VersionFormatValidation()
        {
            var messages = GetValidationMessagesForRule<APIVersionPattern>("version-validation.json");
            Assert.Equal(messages.Count(), 1);
        }

        [Fact]
        public void GuidUsageValidation()
        {
            var messages = GetValidationMessagesForRule<GuidUsage>("guid-validation.json");
            Assert.Equal(messages.Count(), 1);
        }

        [Fact]
        public void DeleteRequestBodyValidation()
        {
            var messages = GetValidationMessagesForRule<DeleteMustNotHaveRequestBody>("delete-request-body-validation.json");
            Assert.Equal(messages.Count(), 1);
        }

        [Fact]
        public void OperationsApiValidation()
        {
            var messages = GetValidationMessagesForRule<OperationsAPIImplementation>("operations-api-validation.json");
            Assert.Equal(messages.Count(), 1);
        }

        [Fact]
        public void ResourceModelValidation()
        {
            var messages = GetValidationMessagesForRule<RequiredPropertiesMissingInResourceModel>("ext-resource-validation.json");
            Assert.Equal(messages.Count(), 1);
        }

        [Fact]
        public void SkuModelValidation()
        {
            var messages = GetValidationMessagesForRule<InvalidSkuModel>("skumodel-validation.json");
            Assert.Equal(messages.Count(), 1);
        }

        [Fact]
        public void TrackedResourceGetOperationValidation2()
        {
            var messages = GetValidationMessagesForRule<TrackedResourceGetOperation>("tracked-resource-1-validation.json");
            Assert.Equal(messages.Count(), 1);
        }

        [Fact]
        public void TrackedResourceListByResourceGroupValidation()
        {
            var messages = GetValidationMessagesForRule<TrackedResourceListByResourceGroup>("tracked-resource-2-validation.json");
            Assert.Equal(messages.Count(), 1);
        }

        [Fact]
        public void TrackedResourcePatchOperationValidationValidation()
        {
            var messages = GetValidationMessagesForRule<TrackedResourcePatchOperation>("tracked-resource-patch-operation.json");
            Assert.Equal(messages.Count(), 1);
        }

        [Fact]
        public void TrackedResourceGetOperationValidation()
        {
            var messages = GetValidationMessagesForRule<TrackedResourceGetOperation>("tracked-resource-get-operation.json");
            Assert.Equal(messages.Count(), 1);
        }

        [Fact]
        public void TrackedResourceListBySubscriptionValidation()
        {
            var messages = GetValidationMessagesForRule<TrackedResourceListBySubscription>("tracked-resource-3-validation.json");
            Assert.Equal(messages.Count(), 1);
        }

        [Fact]
        public void TrackedResourceListByImmediateParentValidation()
        {
            var messages = GetValidationMessagesForRule<TrackedResourceListByImmediateParent>("list-by-immediate-parent.json");
            Assert.Equal(messages.Count(), 1);
        }

        [Fact]
        public void TrackedResourceListByImmediateParentWithOperationValidation()
        {
            var messages = GetValidationMessagesForRule<TrackedResourceListByImmediateParent>("list-by-immediate-parent-2.json");
            Assert.Equal(messages.Count(), 1);
        }

        [Fact]
        public void PutGetPatchResponseValidation()
        {
            var messages = GetValidationMessagesForRule<PutGetPatchResponseSchema>("putgetpatch-response-validation.json");
            Assert.Equal(messages.Count(), 1);
        }

        [Fact]
        public void DefaultValuedInPropertiesInPatchRequestValidation()
        {
            // This test validates if a definition has required properties which are marked as readonly true
            var messages = GetValidationMessagesForRule<PatchBodyParametersSchema>("default-valued-properties-in-patch-request.json");
            Assert.Equal(messages.Count(), 1);
        }

        [Fact]
        public void RequiredPropertiesInPatchRequestValidation()
        {
            // This test validates if a definition has required properties which are marked as readonly true
            var messages = GetValidationMessagesForRule<PatchBodyParametersSchema>("req-properties-in-patch-request.json");
            Assert.Equal(messages.Count(), 1);
        }

        [Fact]
        public void PutResponseResourceValidationTest()
        {
            var messages = GetValidationMessagesForRule<XmsResourceInPutResponse>("put-response-resource-validation.json");
            Assert.Equal(messages.Count(), 1);
        }
    }

    #region Positive tests

    public partial class OpenAPIModelerValidationTests
    {
        /// <summary>
        /// Verifies that a clean OpenAPI file does not result in any validation errors
        /// </summary>
        [Fact]
        public void CleanFileValidation()
        {
            // individual state
            var subtest1md = new ServiceDefinitionMetadata
            {
                ServiceDefinitionDocumentType = ServiceDefinitionDocumentType.ARM,
                MergeState = ServiceDefinitionDocumentState.Individual
            };
            var messages = this.ValidateOpenAPISpec(Path.Combine(AutoRest.Core.Utilities.Extensions.CodeBaseDirectory, "Resource", "OpenAPI", "Validation", "positive", "clean-complex-spec.json"), subtest1md);
            Assert.Empty(messages.Where(m => m.Severity >= Category.Warning));

            // composed state
            var subtest2md = new ServiceDefinitionMetadata
            {
                ServiceDefinitionDocumentType = ServiceDefinitionDocumentType.ARM,
                MergeState = ServiceDefinitionDocumentState.Composed
            };
            messages = this.ValidateOpenAPISpec(Path.Combine(AutoRest.Core.Utilities.Extensions.CodeBaseDirectory, "Resource", "OpenAPI", "Validation", "positive", "clean-complex-spec.json"), subtest2md);
            Assert.Empty(messages.Where(m => m.Severity >= Category.Warning));

        }

        /// <summary>
        /// 
        /// </summary>
        [Fact]
        public void ValidCollectionObjectsPropertiesName()
        {
            var messages = GetValidationMessagesForRule<CollectionObjectPropertiesNaming>(Path.Combine("positive", "collection-objects-naming-valid.json"));
            Assert.Empty(messages);
        }

        /// <summary>
        /// Verifies that tracked resource has a patch operation
        /// </summary>
        [Fact]
        public void ValidTrackedResourcePatchOperation()
        {
            var messages = GetValidationMessagesForRule<TrackedResourcePatchOperation>(Path.Combine("positive", "tracked-resource-patch-valid-operation.json"));
            Assert.Empty(messages);
        }

        /// <summary>
        /// Verifies that tracked resource has a get operation
        /// </summary>
        [Fact]
        public void ValidTrackedResourceGetOperation()
        {
            var messages = GetValidationMessagesForRule<TrackedResourceGetOperation>(Path.Combine("positive", "tracked-resource-get-valid-operation.json"));
            Assert.Empty(messages);
        }

        /// <summary>
        /// Verifies that property names follow camelCase style
        /// </summary>
        [Fact]
        public void ValidPropertyNameCasing()
        {
            var messages = GetValidationMessagesForRule<BodyPropertiesNamesCamelCase>(Path.Combine("positive", "property-names-casing-valid.json"));
            Assert.Empty(messages);
            messages = GetValidationMessagesForRule<DefinitionsPropertiesNamesCamelCase>(Path.Combine("positive", "property-names-casing-valid.json"));
            Assert.Empty(messages);
        }

        [Fact]
        public void ValidArmResourcePropertiesBag()
        {
            var messages = GetValidationMessagesForRule<ArmResourcePropertiesBag>(Path.Combine("positive", "arm-resource-properties-valid.json"));
            Assert.Empty(messages);
        }

        /// <summary>
        /// Verifies resource models are correctly identified
        /// </summary>
        [Fact]
        public void ValidResourceModels()
        {
            var filePath = Path.Combine(AutoRest.Core.Utilities.Extensions.CodeBaseDirectory, "Resource", "OpenAPI", "Validation", "positive", "valid-resource-model-definitions.json");
            var fileText = File.ReadAllText(filePath);
            var servDef = SwaggerParser.Parse(filePath, fileText);
            Uri uriPath;
            Uri.TryCreate(filePath, UriKind.RelativeOrAbsolute, out uriPath);
            var context = new RuleContext(servDef, uriPath);
            Assert.Equal(4, context.ResourceModels.Count());
            Assert.Equal(1, context.TrackedResourceModels.Count());
            Assert.Equal(3, context.ProxyResourceModels.Count());
        }

        /// <summary>
        /// Verifies that sku object
        /// </summary>
        [Fact]
        public void ValidSkuObjectStructure()
        {
            var messages = GetValidationMessagesForRule<InvalidSkuModel>(Path.Combine("positive", "skumodel-validation-valid.json"));
            Assert.Empty(messages);
        }

        /// <summary>
        /// Verifies resource model readonly properties
        /// </summary>
        [Fact]
        public void ValidResourceModelReadOnlyProperties()
        {
            var messages = GetValidationMessagesForRule<RequiredPropertiesMissingInResourceModel>(Path.Combine("positive", "valid-resource-model-readonly-props.json"));
            Assert.Empty(messages);
        }
    }

    #endregion
}
