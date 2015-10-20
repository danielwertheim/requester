using System.Net;

namespace Requester.Validation
{
    public class HttpTextResponseValidation : HttpResponseValidation<HttpTextResponseValidation, HttpTextResponse>
    {
        public HttpTextResponseValidation(HttpTextResponse response) : base(response) {}

        public JsonHttpResponseValidation BeJsonResponse()
        {
            return new JsonHttpResponseValidation(Response);
        }
    }

    public class HttpEntityResponseValidation<TEntity> : HttpResponseValidation<HttpEntityResponseValidation<TEntity>, HttpEntityResponse<TEntity>> where TEntity : class
    {
        public HttpEntityResponseValidation(HttpEntityResponse<TEntity> response) : base(response) { }
    }

    public abstract class HttpResponseValidation<TSelf, TResponse> where TSelf : HttpResponseValidation<TSelf, TResponse> where TResponse : HttpResponse
    {
        public TResponse Response { get; }

        protected HttpResponseValidation(TResponse response)
        {
            if(response == null)
                throw AssertionExceptionFactory.Create("Expected response to be an instance, but got NULL.");

            Response = response;
        }

        public TSelf HaveStatus(HttpStatusCode status)
        {
            if (Response.StatusCode != status)
                throw AssertionExceptionFactory.CreateForResponse(Response, "Expected status to be '{0}', but got '{1}'.", status, Response.StatusCode);

            return this as TSelf;
        }

        public TSelf BeSuccessful()
        {
            if (!Response.IsSuccess)
                throw AssertionExceptionFactory.CreateForResponse(Response, "Expected response to be successful, but it was failed.");

            return this as TSelf;
        }

        public TSelf HaveFailed()
        {
            if (Response.IsSuccess)
                throw AssertionExceptionFactory.CreateForResponse(Response, "Expected response to have failed, but it succeeded.");

            return this as TSelf;
        }
    }
}