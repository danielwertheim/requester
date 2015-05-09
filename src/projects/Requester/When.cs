using System;
using System.Net.Http;
using EnsureThat;
using Requester.Http;

namespace Requester
{
    public class When
    {
        public static HttpResponse Head(string url, Action<HttpRequest> configurer = null)
        {
            return DoRequest(HttpMethod.Head, url, configurer);
        }

        public static HttpResponse Put(string url, Action<HttpRequest> configurer = null)
        {
            return DoRequest(HttpMethod.Put, url, configurer);
        }

        public static HttpResponse Delete(string url, Action<HttpRequest> configurer = null)
        {
            return DoRequest(HttpMethod.Delete, url, configurer);
        }

        public static HttpResponse PostOfJson(string url, string content, Action<HttpRequest> configurer = null)
        {
            Ensure.That(content, "content").IsNotNull();

            return DoRequest(HttpMethod.Post, url, configurer, content);
        }

        public static HttpResponse PutOfJson(string url, string content, Action<HttpRequest> configurer = null)
        {
            Ensure.That(content, "content").IsNotNull();

            return DoRequest(HttpMethod.Put, url, configurer, content);
        }

        public static HttpResponse GetOfJson(string url, Action<HttpRequest> configurer = null)
        {
            return DoRequest(HttpMethod.Get, url, configurer);
        }

        private static HttpResponse DoRequest(HttpMethod method, string url, Action<HttpRequest> configurer, string content = null)
        {
            Ensure.That(url, "url").IsNotNullOrWhiteSpace();

            var request = new HttpRequest(method);

            if(!string.IsNullOrWhiteSpace(content))
                request.WithJsonContent(content);

            if (configurer != null)
                configurer(request);

            using (var requester = new HttpRequester(url))
                return requester.SendAsync(request).Result;
        }
    }
}