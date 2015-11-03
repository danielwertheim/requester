using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;

namespace Requester.Extensions
{
    public static class HttpExtensions
    {
        public static string GetETag(this HttpResponseHeaders headers)
        {
            IEnumerable<string> values;
            if (!headers.TryGetValues("ETag", out values))
                return null;

            var eTag = values.FirstOrDefault();

            return !string.IsNullOrWhiteSpace(eTag)
                ? eTag.TrimStart('"').TrimEnd('"')
                : null;
        }

        public static string GetLocation(this HttpResponseHeaders headers)
        {
            IEnumerable<string> values;

            return !headers.TryGetValues("Location", out values)
                ? null
                : values.FirstOrDefault();
        }
    }
}