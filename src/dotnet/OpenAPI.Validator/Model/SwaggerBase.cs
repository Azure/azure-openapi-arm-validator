// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System.Collections.Generic;
using Newtonsoft.Json;
using OpenAPI.Validator.Validation.Extensions;
using OpenAPI.Validator.Validation.Core;

namespace OpenAPI.Validator.Model
{
    public abstract class SwaggerBase
    {
        protected SwaggerBase()
        {
            Extensions = new Dictionary<string, object>();
        }

        /// <summary>
        /// Vendor extensions.
        /// </summary>
        [JsonExtensionData]
        public Dictionary<string, object> Extensions { get; set; }
    }
}