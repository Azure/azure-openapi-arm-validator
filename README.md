# Contributing

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.

## Build dependencies
- Node (6.9.5 or higher)
- Node Package Manager
- Typescript (2.3.0 or higher)

## Build scripts
### How to build
The first step would be to run ```npm install``` so we have all the required modules installed.
#### How to build the whole repo
```
gulp build
```
#### How to build the typescript repo
```
gulp build/typescript
```
#### How to build the dotnet repo
To restore the dotnet packages
```
gulp restore/dotnet
```
This builds the dotnet projects.
```
gulp build/dotnet
```
### How to test
To run all tests under the repo
```
gulp test
```
#### How to run the typescript tests
```
gulp test/typescript
```
#### How to run the dotnet tests
```
gulp test/dotnet
```
#### How to debug the dotnet repo

This extension runs the validation on two threads:
 - Individual merge state: validates each individual OpenAPI file.
 - Composed merge state: validates the full OpenAPI spec.

For debugging both composed and individual merge state at the same time
```
autorest input/file --validation --azure-arm-validator --use=path/to/your/repo/sr
c/dotnet/AutoRest --azure-arm-validator.debugger 
```
For debugging composed merge state
```
autorest input/file --validation --azure-arm-validator --use=path/to/your/repo/sr
c/dotnet/AutoRest --azure-arm-validator.composed-debugger 
```
For debugging individual merge state
```
autorest --input-file=path/to/swagger --validation --azure-arm-validator --use=path/to/your/repo/sr
c/dotnet/AutoRest --azure-arm-validator.individual-debugger 
```
#### How to debug the typescript repo
Temporarily change the start script in `src/typescript/package.json` to `"start": "node --inspect-brk ./index.js"`.
Then run:
```
autorest input/file --validation --azure-arm-validator --use=path/to/your/repo/src/typescript
```
The typescript repo can be debugged with VS Code or Chrome.
### How to write a new validation rule using typescript
1. Add a typescript file under ```azure-openapi-validator``` directory named same as the name of the rule. Add the ```id```, ```name```, ```severity```, ```category```,  ```mergeState```,  ```openapiType```,  ```appliesTo_JsonQuery``` properties to the rule. ```appliesTo_JsonQuery``` is the node(s) to which the rule needs to be applied. This is evaluated using JsonPaths. Please refer [here](https://www.npmjs.com/package/jsonpath#jsonpath-syntax) for a brief tutorial about JsonPaths.
2. Next, implement the ```run``` method under the rule that actually does the validation. Add a reference to this script file under ```src/typescript/azure-openapi-validator/index.ts```.
3. Lastly add a test case for the validation rule, by adding a test json/yaml under ```src/typescript/azure-openapi-validator/tests/resources``` and a script under ```src/typescript/azure-openapi-validator/tests``` depending on the type of the validation rule.

### How to publish

