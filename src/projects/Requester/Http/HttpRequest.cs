using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using EnsureThat;

namespace Requester.Http
{
    public class HttpRequest
    {
        public HttpMethod Method { get; private set; }
        public string RelativeUrl { get; private set; }
        public IDictionary<string, string> Headers { get; private set; }
        public HttpContent Content { get; private set; }

        public HttpRequest(HttpMethod method) : this(method, "/") { }

        public HttpRequest(HttpMethod method, string relativeUrl)
        {
            Ensure.That(relativeUrl, "relativeUrl").IsNotNullOrWhiteSpace();

            RelativeUrl = relativeUrl;
            Method = method;
            Headers = new Dictionary<string, string> { { HttpHeaders.Instance.Accept, HttpContentTypes.Instance.ApplicationJson } };
        }

        public virtual HttpRequest WithAccept(Func<HttpContentTypes, string> picker)
        {
            return WithAccept(picker(HttpContentTypes.Instance));
        }

        public virtual HttpRequest WithAccept(string value)
        {
            return WithHeader(HttpHeaders.Instance.Accept, value);
        }

        public virtual HttpRequest WithIfMatch(string value)
        {
            return WithHeader(HttpHeaders.Instance.IfMatch, value);
        }

        public virtual HttpRequest WithHeader(Func<HttpHeaders, string> picker, string value)
        {
            return WithHeader(picker(HttpHeaders.Instance), value);
        }

        public virtual HttpRequest WithHeader(string name, string value)
        {
            Headers[name] = value;

            return this;
        }

        public virtual HttpRequest WithBasicAuthorization(string username, string password)
        {
            return WithBasicAuthorization(new BasicAuthorizationString(username, password));
        }

        public virtual HttpRequest WithBasicAuthorization(BasicAuthorizationString value)
        {
            WithHeader(HttpHeaders.Instance.Authorization, value);

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
            if (!url.StartsWith("/"))
                url = "/" + url;

            if (fmtArgs == null || !fmtArgs.Any())
                RelativeUrl = url;
            else
                RelativeUrl = string.Format(url, fmtArgs);

            return this;
        }
    }
}