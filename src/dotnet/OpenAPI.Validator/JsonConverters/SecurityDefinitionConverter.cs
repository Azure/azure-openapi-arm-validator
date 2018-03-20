﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

using System;
using OpenAPI.Validator.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace OpenAPI.Validator.JsonConverters
{
    public class SecurityDefinitionConverter : SwaggerJsonConverter
    {
        /// <summary>Reads the JSON representation of the object.</summary>
        /// <param name="reader">The <see cref="T:Newtonsoft.Json.JsonReader" /> to read from.</param>
        /// <param name="objectType">Type of the object.</param>
        /// <param name="existingValue">The existing value of object being read.</param>
        /// <param name="serializer">The calling serializer.</param>
        /// <returns>The object value.</returns>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var jo = JObject.Load(reader);
            var securityDefinition = JsonConvert.DeserializeObject<SecurityDefinition>(jo.ToString(), GetSettings(serializer));

            var value = jo.Property("type").Value.Value<string>();
            securityDefinition.SecuritySchemeType = (SecuritySchemeType)Enum.Parse(typeof(SecuritySchemeType), value, true);

            return securityDefinition;
        }

        /// <summary>
        /// Determines whether this instance can convert the specified object type.
        /// </summary>
        /// <param name="objectType">Type of the object.</param>
        /// <returns>
        /// 	<c>true</c> if this instance can convert the specified object type; otherwise, <c>false</c>.
        /// </returns>
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(SecurityDefinition);
        }
    }
}