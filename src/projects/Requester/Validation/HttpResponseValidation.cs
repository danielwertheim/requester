using System.Net;
using Requester.Http;

namespace Requester.Validation
{
    public class HttpResponseValidation
    {
        public HttpResponse Response { get; private set; }

        public HttpResponseValidation(HttpResponse response)
        {
            if(response == null)
                throw AssertionExceptionFactory.Create("Expected response to be an instance, but got NULL.");

            Response = response;
        }

        public HttpResponseValidation HaveStatus(HttpStatusCode status)
        {
            if (Response.StatusCode != status)
                throw AssertionExceptionFactory.Create(Response, "Expected status to be '{0}', but got '{1}'.", status, Response.StatusCode);

            return this;
        }

        public HttpResponseValidation BeSuccessful()
        {
            if (!Response.IsSuccess)
                throw AssertionExceptionFactory.Create(Response, "Expected response to be successful, but it was failed.");

            return this;
        }

        public HttpResponseValidation HaveFailed()
        {
            if (Response.IsSuccess)
                throw AssertionExceptionFactory.Create(Response, "Expected response to have failed, but it succeeded.");

            return this;
        }

        public JsonHttpResponseValidation BeJsonResponse()
        {
            return new JsonHttpResponseValidation(Response);
        }
    }
}