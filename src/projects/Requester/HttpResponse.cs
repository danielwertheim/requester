using System;
using System.Net;
using System.Net.Http;
using Requester.Serialization;

namespace Requester
{
    public class HttpTextResponse : HttpContentResponse<string>
    {
        public override bool HasContent => !string.IsNullOrWhiteSpace(Content);
    }

    public class HttpEntityResponse<TEntity> : HttpContentResponse<TEntity> where TEntity : class { }

    public abstract class HttpContentResponse<TContent> : HttpResponse where TContent : class
    {
        public TContent Content { get; set; }
        public string ContentType { get; set; }
        public long? ContentLength { get; set; }
        public virtual bool HasContent => Content != null;

        public override string ToStringDebugVersion()
        {
            return string.Format("{1}{0}ContentType:{2}{0}HasContent:{3}{0}Content:{4}{0}",
                Environment.NewLine,
                base.ToStringDebugVersion(),
                ContentType,
                HasContent,
                NullString.IfNull(GetContentStringForDebug()));
        }

        protected virtual string GetContentStringForDebug()
        {
            return Content is string ? Content as string : DebugJsonSerializer.Instance.Serialize(Content);
        }
    }

    public class HttpResponse
    {
        public HttpStatusCode StatusCode { get; set; }
        public string ETag { get; set; }
        public Uri RequestUri { get; set; }
        public HttpMethod RequestMethod { get; set; }
        public string Reason { get; set; }
        public bool IsSuccess => (int)StatusCode >= 200 && (int)StatusCode < 300;

        public override string ToString()
        {
#if DEBUG
            return ToStringDebugVersion();
#else
            return base.ToString();
#endif
        }

        public virtual string ToStringDebugVersion()
        {
            return string.Format("RequestUri: {1}{0}RequestMethod: {2}{0}Status: {3}({4}){0}Reason: {5}{0}ETag: {6}{0}",
                Environment.NewLine,
                RequestUri,
                RequestMethod,
                StatusCode,
                (int)StatusCode,
                NullString.IfNull(Reason),
                NullString.IfNull(ETag));
        }
    }
}