"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
/*---------------------------------------------------------------------------------------------
 *  Copyright (c) Microsoft Corporation. All rights reserved.
 *  Licensed under the MIT License. See License.txt in the project root for license information.
 *--------------------------------------------------------------------------------------------*/
const rule_1 = require("../rule");
exports.XmsParameterExtensionInGlobalParameters = "XmsParameterExtensionInGlobalParameters";
const jp = require('jsonpath');
rule_1.rules.push({
    id: "R2010",
    name: exports.XmsParameterExtensionInGlobalParameters,
    severity: "information",
    category: "SDKViolation",
    mergeState: rule_1.MergeStates.individual,
    openapiType: rule_1.OpenApiTypes.arm,
    appliesTo_JsonQuery: "$.parameters.*",
    run: function* (doc, node, path) {
        doc = doc;
        node = node;
        path = path;
    }
});
