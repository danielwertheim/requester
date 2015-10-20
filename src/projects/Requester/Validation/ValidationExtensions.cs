using System;

namespace Requester.Validation
{
    public static class ValidationExtensions
    {
        public static HttpTextResponse TheResponse(this HttpTextResponse response, Action<HttpTextResponseValidation> should)
        {
            should(new HttpTextResponseValidation(response));

            return response;
        }

        public static HttpEntityResponse<TEntity> TheResponse<TEntity>(this HttpEntityResponse<TEntity> response, Action<HttpEntityResponseValidation<TEntity>> should) where TEntity : class
        {
            should(new HttpEntityResponseValidation<TEntity>(response));

            return response;
        }
    }
}