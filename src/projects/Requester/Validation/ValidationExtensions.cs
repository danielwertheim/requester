using System;

namespace Requester.Validation
{
    public static class ValidationExtensions
    {
        public static HttpTextResponse TheResponse(this HttpTextResponse response, Action<HttpResponseValidation> should)
        {
            should(new HttpResponseValidation(response));

            return response;
        }
    }
}