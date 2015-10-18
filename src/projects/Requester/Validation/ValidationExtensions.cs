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

        //TODO: Fix
        //public static HttpTextResponse TheResponse<TEntity>(this HttpEntityResponse<TEntity> response, Action<HttpResponseValidation> should) where TEntity : class
        //{
        //    should(new HttpResponseValidation(response));

        //    return response;
        //}
    }
}