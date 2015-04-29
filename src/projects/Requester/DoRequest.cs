using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using EnsureThat;
using Requester.Extensions;

namespace Requester
{
    public class DoRequest : IDoRequest
    {
        protected readonly Uri BaseAddress;
        protected readonly Dictionary<string, string> Headers;
        protected HttpContent Content;

        public static Encoding DefaultEncoding { private get; set; }
        public static Func<string, IDoRequest> Factory { private get; set; }
        public static Func<IDoRequest, IDoRequest> Initializer { private get; set; }

        static DoRequest()
        {
            Reset();
        }

        public static void Reset()
        {
            DefaultEncoding = Encoding.UTF8;
            Factory = uri => new DoRequest(uri);
            Initializer = request => request.WithAccept(i => i.ApplicationJson);
        }

        protected DoRequest(string uri)
        {
            Ensure.That(uri, "uri").IsNotNullOrWhiteSpace();

            var tmpUri = new Uri(uri);

            Headers = new Dictionary<string, string>();
            BaseAddress = new Uri(tmpUri.GetAbsoluteAddressExceptUserInfo().TrimEnd(new[] { '/' }));

            var basicAuthString = tmpUri.GetBasicAuthString();
            if (basicAuthString != null)
                Headers[HttpHeaders.Instance.Authorization] = basicAuthString;
        }

        public static IDoRequest Against(string uri)
        {
            return Initializer(Factory(uri));
        }

        public virtual IDoRequest WithAccept(Func<ContentTypes, string> picker)
        {
            return WithAccept(picker(ContentTypes.Instance));
        }

        public virtual IDoRequest WithAccept(string value)
        {
            return WithHeader(HttpHeaders.Instance.Accept, value);
        }

        public virtual IDoRequest WithIfMatch(string value)
        {
            return WithHeader(HttpHeaders.Instance.IfMatch, value);
        }

        public virtual IDoRequest WithHeader(Func<HttpHeaders, string> picker , string value)
        {
            return WithHeader(picker(HttpHeaders.Instance), value);
        }

        public virtual IDoRequest WithHeader(string name, string value)
        {
            Headers[name] = value;

            return this;
        }

        public virtual IDoRequest WithBasicAuthorization(string username, string password)
        {
            return WithBasicAuthorization(new BasicAuthorizationString(username, password));
        }

        public virtual IDoRequest WithBasicAuthorization(BasicAuthorizationString value)
        {
            WithHeader(HttpHeaders.Instance.Authorization, value);

            return this;
        }

        public virtual IDoRequest WithJsonContent(string content)
        {
            Content = new StringContent(content, DefaultEncoding, ContentTypes.Instance.ApplicationJson);

            return this;
        }

        public virtual Task<HttpResponse> UsingHeadAsync(string relativeUrl = null)
        {
            return SendAsync(HttpMethod.Head, relativeUrl);
        }

        public virtual Task<HttpResponse> UsingGetAsync(string relativeUrl = null)
        {
            return SendAsync(HttpMethod.Get, relativeUrl);
        }

        public virtual Task<HttpResponse> UsingPostAsync(string relativeUrl = null)
        {
            return SendAsync(HttpMethod.Post, relativeUrl);
        }

        public virtual Task<HttpResponse> UsingPutAsync(string relativeUrl = null)
        {
            return SendAsync(HttpMethod.Put, relativeUrl);
        }

        public virtual Task<HttpResponse> UsingDeleteAsync(string relativeUrl = null)
        {
            return SendAsync(HttpMethod.Delete, relativeUrl);
        }

        protected virtual async Task<HttpResponse> SendAsync(HttpMethod method, string relativeUrl)
        {
            using (var request = CreateHttpRequest(method, GenerateRequestUri(relativeUrl)))
                return await CreateHttpResponseAsync(request).ForAwait();
        } 

        protected virtual HttpRequestMessage CreateHttpRequest(HttpMethod method, Uri uri)
        {
            var request = new HttpRequestMessage(method, uri);

            foreach (var header in Headers)
                request.Headers.TryAddWithoutValidation(header.Key, header.Value);

            if ((method == HttpMethod.Put || method == HttpMethod.Post) && Content != null)
                request.Content = Content;

            return request;
        }

        protected virtual Uri GenerateRequestUri(string relativeUrl = null)
        {
            return relativeUrl == null
                ? BaseAddress
                : new Uri(string.Format("{0}/{1}", BaseAddress.ToString().TrimEnd('/'), relativeUrl.TrimStart('/')));
        }

        protected virtual async Task<HttpResponse> CreateHttpResponseAsync(HttpRequestMessage request)
        {
            var response = new HttpResponse
            {
                RequestUri = request.RequestUri,
                RequestMethod = request.Method
            };

            var handler = new HttpClientHandler
            {
                AllowAutoRedirect = false
            };

            using (var client = new HttpClient(handler, true))
            {
                using (var message = await client.SendAsync(request).ForAwait())
                {
                    if (ShouldFollowResponse(message))
                    {
                        request.RequestUri = message.Headers.Location;
                        return await CreateHttpResponseAsync(request).ForAwait();
                    }

                    response.StatusCode = message.StatusCode;
                    response.Reason = message.ReasonPhrase;

                    if (message.Content != null)
                    {
                        if (message.Content.Headers != null)
                        {
                            response.ETag = message.Headers.GetETag();
                            response.ContentLength = message.Content.Headers.ContentLength;
                            
                            if(message.Content.Headers.ContentType != null)
                                response.ContentType = message.Content.Headers.ContentType.MediaType;
                            
                            response.Content = await message.Content.ReadAsStringAsync().ForAwait();
                            if (response.Content == string.Empty)
                                response.Content = null;
                        }
                    }
                }
            }

            return response;
        }

        protected virtual bool ShouldFollowResponse(HttpResponseMessage response)
        {
            return response.StatusCode == HttpStatusCode.MovedPermanently && response.Headers.Location != null;
        }
    }
}