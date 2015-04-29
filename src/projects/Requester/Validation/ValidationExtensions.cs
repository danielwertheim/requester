namespace Requester.Validation
{
    public static class ValidationExtensions
    {
        public static HttpResponseValidation IsExpectedTo(this HttpResponse response)
        {
            return new HttpResponseValidation(response);
        }
    }
}