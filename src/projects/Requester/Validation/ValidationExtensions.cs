using System;
using Requester.Http;

namespace Requester.Validation
{
    public static class ValidationExtensions
    {
        public static HttpResponse TheResponse(this HttpResponse response, Action<HttpResponseValidation> should)
        {
            should(new HttpResponseValidation(response));

            return response;
        }
    }
}