using System;
using System.Net;
using System.Net.Http;

namespace Requester
{
    public class HttpResponse
    {
        public HttpStatusCode StatusCode { get; set; }
        public string ETag { get; set; }
        public Uri RequestUri { get; set; }
        public HttpMethod RequestMethod { get; set; }
        public string Content { get; set; }
        public string ContentType { get; set; }
        public long? ContentLength { get; set; }
        public bool HasContent { get { return !string.IsNullOrWhiteSpace(Content); } }
        public string Reason { get; set; }
        public bool IsSuccess
        {
            get { return (int)StatusCode >= 200 && (int)StatusCode < 300; }
        }

        public override string ToString()
        {
#if DEBUG
            return ToStringDebugVersion();
#else
            return base.ToString();
#endif
        }

        public virtual string ToStringDebugVersion(bool includeContent = false)
        {
            return string.Format("RequestUri: {1}{0}RequestMethod: {2}{0}Status: {3}({4}){0}Reason: {5}{0}ETag: {6}{0}ContentType:{7}{0}HasContent:{8}{0}Content:{9}{0}",
                Environment.NewLine,
                RequestUri,
                RequestMethod,
                StatusCode,
                (int)StatusCode,
                NullString.IfNull(Reason),
                NullString.IfNull(ETag),
                ContentType,
                HasContent,
                includeContent ? NullString.IfNull(Content) : "<NOT BEING SHOWED>");
        }
    }
}