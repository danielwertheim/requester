using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using Requester.Http;

namespace Requester
{
    public class HttpRequest
    {
        public HttpMethod Method { get; private set; }
        public string RelativeUrl { get; protected set; }

        public IDictionary<string, string> Headers { get; }
        public HttpContent Content { get; private set; }

        public HttpRequest(HttpMethod method, string relativeUrl = null)
        {
            RelativeUrl = relativeUrl;
            Method = method;
            Headers = new Dictionary<string, string>
            {
                { HttpRequesterHeaders.Instance.Accept, HttpContentTypes.Instance.ApplicationJson }
            };
        }

        public virtual HttpRequest WithAccept(Func<HttpContentTypes, string> picker)
        {
            return WithAccept(picker(HttpContentTypes.Instance));
        }

        public virtual HttpRequest WithAccept(string value)
        {
            return WithHeader(HttpRequesterHeaders.Instance.Accept, value);
        }

        public virtual HttpRequest WithIfMatch(string value)
        {
            return WithHeader(HttpRequesterHeaders.Instance.IfMatch, value);
        }

        public virtual HttpRequest WithIfNoneMatch(string value)
        {
            return WithHeader(HttpRequesterHeaders.Instance.IfNoneMatch, value);
        }

        public virtual HttpRequest WithHeader(Func<HttpRequesterHeaders, string> picker, string value)
        {
            return WithHeader(picker(HttpRequesterHeaders.Instance), value);
        }

        public virtual HttpRequest WithHeader(string name, string value)
        {
            Headers[name] = value;

            return this;
        }

        public virtual HttpRequest WithAuthorization(string value)
        {
            return WithHeader(HttpRequesterHeaders.Instance.Authorization, value);
        }

        public virtual HttpRequest WithBearer(string value)
        {
            return WithHeader(HttpRequesterHeaders.Instance.Authorization, "Bearer " + value);
        }

        public virtual HttpRequest WithBasicAuthorization(string username, string password)
        {
            return WithBasicAuthorization(new BasicAuthorizationString(username, password));
        }

        public virtual HttpRequest WithBasicAuthorization(BasicAuthorizationString value)
        {
            return WithHeader(HttpRequesterHeaders.Instance.Authorization, value);
        }

        public virtual HttpRequest WithContent<T>(T content) where T : HttpContent
        {
            if (content != null)
                Content = content;

            return this;
        }

        public virtual HttpRequest WithContent(string content, Func<HttpContentTypes, string> picker)
        {
            return WithContent(content, picker(HttpContentTypes.Instance));
        }

        public virtual HttpRequest WithContent(string content, string contentType)
        {
            if(!string.IsNullOrWhiteSpace(content))
                Content = new BytesContent(Encoding.UTF8.GetBytes(content), contentType);

            return this;
        }

        public virtual HttpRequest WithContent(byte[] content, Func<HttpContentTypes, string> picker)
        {
            return WithContent(content, picker(HttpContentTypes.Instance));
        }

        public virtual HttpRequest WithContent(byte[] content, string contentType)
        {
            if (content != null && content.Length > 0)
                Content = new BytesContent(content, contentType);

            return this;
        }

        public virtual HttpRequest WithJsonContent(string content = null)
        {
            Content = string.IsNullOrWhiteSpace(content)
                ? new JsonContent()
                : new JsonContent(content);

            return this;
        }

        public virtual HttpRequest WithRelativeUrl(string url, params object[] fmtArgs)
        {
            if (fmtArgs != null && !fmtArgs.Any())
                url = string.Format(url, fmtArgs);

            RelativeUrl = url;

            return this;
        }
    }
}