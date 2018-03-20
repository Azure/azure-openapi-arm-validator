# Typescript Azure OpenAPI validator

Azure OpenAPI validator (Typescript)

## Validation

``` yaml $(azure-arm-validator)
pipeline:
  swagger-document/openapi-arm-validator:
    input: swagger-document/identity
    scope: azure-arm-validator-composed
  swagger-document/individual/openapi-arm-validator:
    input: swagger-document/individual/identity
    scope: azure-arm-validator-individual
    
```

``` yaml $(azure-arm-validator)
azure-arm-validator-composed:
  merge-state: composed
azure-arm-validator-individual:
  merge-state: individual
```