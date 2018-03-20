﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using AutoRest.Core.Utilities;
using OpenAPI.Validator.Validation;
using OpenAPI.Validator.Validation.Core;

namespace OpenAPI.Validator.Model
{
    /// <summary>
    /// Allows referencing an external resource for extended documentation.
    /// </summary>
    public class ExternalDoc
    {
        private string _description;

        /// <summary>
        /// Url of external Swagger doc.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Description of external Swagger doc.
        /// </summary>
        public string Description
        {
            get { return _description; }
            set { _description = value.StripControlCharacters(); }
        }
    }
}