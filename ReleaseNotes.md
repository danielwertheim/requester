# Release notes

## v2.0.0 - 2017-07-05
- **[Changed]**: Now targets .NET Standard v1.3
- **[Changed]**: Since Newtonsoft JSON.Net went from maintaining the JSON Schema validation in the MIT licensed JSON.Net lib and instead started to maintain that in a separate distribution JSON.Net Schema which has another license and requires a commercial license, Requester.Validation now uses [NJsonSchema](https://github.com/RSuter/NJsonSchema) which is MIT licensed.
- **[Changed]**: Requester and Requester.Validation now targets .NET Standard.
- **[Changed]**: Serialization now uses `DefaultContractResolver` in NewtonSoft JSON.Net, but still the `CamelCaseNamingStrategy` but now is explicit with *do not touch dicitionary keys*.
- **[Changed]**: Serialization `MissingMember` should not throw and `TypeNameHandling` should exclude type names.
