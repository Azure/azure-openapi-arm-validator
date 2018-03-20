// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using AutoRest.Core.Logging;
using OpenAPI.Validator.Model;
using OpenAPI.Validator.Validation.Core;
using System.Collections.Generic;

namespace OpenAPI.Validator.Validation.Extensions
{
    internal static class PageableExtensions
    {
        public static T GetValueOrNull<T>(this Dictionary<string, T> dictionary, string key)
        {
            T value = default(T);
            if (dictionary != null)
            {
                dictionary.TryGetValue(key, out value);
            }
            return value;
        }
    }
}
