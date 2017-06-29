using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using NJsonSchema;
using Requester.Extensions;

namespace Requester.Validation
{
    public class JsonHttpResponseValidation
    {
        public HttpTextResponse Response { get; }

        public JsonHttpResponseValidation(HttpTextResponse response)
        {
            if (response == null)
                throw AssertionExceptionFactory.Create("Expected response to be an instance, but got NULL.");

            const string expectedContentType = "application/json";
            if (response.ContentType != expectedContentType)
                throw AssertionExceptionFactory.CreateForResponse(response, "Expected response content type to be '{0}', but got '{1}'.", expectedContentType, NullString.IfNull(response.ContentType));

            Response = response;
        }

        public JsonHttpResponseValidation Match<T>(T entity) where T : class
        {
            var o = JsonConvert.DeserializeObject<T>(Response.Content);
            if (o == null)
                throw AssertionExceptionFactory.CreateForResponse(Response, "Expected response content to match sent entity, but it deserialized to '{0}'", NullString.Value);

            o.ShouldBeValueEqualTo(entity);

            return this;
        }

        public JsonHttpResponseValidation HaveAnyContent()
        {
            if (string.IsNullOrWhiteSpace(Response.Content))
                throw AssertionExceptionFactory.CreateForResponse(Response, "Expected response content to NOT be: NULL, Empty or WhiteSpace.");

            return this;
        }

        public JsonHttpResponseValidation HaveSpecificValueFor<T>(string path, T expectedValue)
        {
            var node = JToken.Parse(Response.Content).SelectToken(path, false);
            if (node == null)
                throw AssertionExceptionFactory.CreateForResponse(Response, "Expected sent path '{0}' to map to a node in the JSON document, but it did not.", path);

            if (!node.ValueIsEqualTo(expectedValue))
                throw AssertionExceptionFactory.CreateForResponse(Response, "Expected sent path '{0}' to return '{1}', but got '{2}'.", path, expectedValue, node.Value<T>());

            return this;
        }

        public JsonHttpResponseValidation NotHavingSpecificValueFor<T>(string path, T unExpectedValue)
        {
            var node = JToken.Parse(Response.Content).SelectToken(path, false);
            if (node == null)
                throw AssertionExceptionFactory.CreateForResponse(Response, "Expected sent path '{0}' to map to a node in the JSON document, but it did not.", path);

            if (node.ValueIsEqualTo(unExpectedValue))
                throw AssertionExceptionFactory.CreateForResponse(Response, "Expected sent path '{0}' to NOT return '{1}', but it sure got '{2}'.", path, unExpectedValue, node.Value<T>());

            return this;
        }

        public async Task<JsonHttpResponseValidation> HaveJsonConformingToSchemaAsync(string jsonSchema)
        {
            var schema = await JsonSchema4.FromJsonAsync(jsonSchema);
            var errors = schema.Validate(Response.Content);
            if (!errors.Any())
                return this;

            var details = new StringBuilder();

            var c = 0;
            foreach (var error in errors)
            {
                details.AppendLine($"Err#{++c}:{error.Path}:{error.Kind}");
            }

            throw AssertionExceptionFactory.CreateForResponse(Response,
                "Expected object to be conforming to specified JSON schema.{0}Details:{0}{1}", Environment.NewLine, details);
        }
    }
}