using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;

namespace Requester.Validation
{
    public class JsonHttpResponseValidation
    {
        public HttpResponse Response { get; private set; }

        public JsonHttpResponseValidation(HttpResponse response)
        {
            if (response == null)
                throw AssertionExceptionFactory.Create("Expected response to be an instance, but got NULL.");

            if (response.ContentType != "application/json")
                throw AssertionExceptionFactory.Create(response, "Expected response content type to be '{0}', but got '{1}'.", NullString.IfNull(response.ContentType));

            Response = response;
        }

        public JsonHttpResponseValidation Match<T>(T entity) where T : class
        {
            var o = JsonConvert.DeserializeObject<T>(Response.Content);
            if (o == null)
                throw AssertionExceptionFactory.Create(Response, "Expected response content to match sent entity, but it deserialized to '{0}'", NullString.Value);

            o.ShouldBeValueEqualTo(entity);

            return this;
        }

        public JsonHttpResponseValidation HaveAnyContent()
        {
            if(string.IsNullOrWhiteSpace(Response.Content))
                throw AssertionExceptionFactory.Create(Response, "Expected response content to NOT be: NULL, Empty or WhiteSpace.");

            return this;
        }

        public JsonHttpResponseValidation HaveSpecificValue<T>(string path, T expectedValue)
        {
            var node = JToken.Parse(Response.Content).SelectToken(path, false);
            if (node == null)
                throw AssertionExceptionFactory.Create(Response, "Expected sent path '{0}' to map to a node in the JSON document, but it did not.", path);

            if(!Equals(node.Value<T>(), expectedValue))
                throw AssertionExceptionFactory.Create(Response, "Expected sent path '{0}' returned '{1}', but expected '{2}'.", path, node.Value<T>(), expectedValue);

            return this;
        }

        public JsonHttpResponseValidation HaveJsonConformingToSchema(string properties)
        {
            if (!properties.StartsWith("{"))
                properties = "{" + properties;

            if (!properties.EndsWith("}"))
                properties += "}";

            var schema = JsonSchema.Parse("{type: 'object', properties:" + properties + "}");
            JToken.Parse(Response.Content).Validate(schema, (sender, args) =>
                AssertionExceptionFactory.Create(Response, "Expected object to be conforming to specified JSON schema, but... {0}", args.Message));

            return this;
        }
    }
}