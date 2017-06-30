using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using EnsureThat;
using Requester.Extensions;
using Requester.Http;
using Requester.Serialization;

namespace Requester
{
    public class HttpRequester : IHttpRequester, IHttpRequesterConfig, IDisposable, IConfigureHttpRequesterOf<HttpRequester>
    {
        protected HttpClient HttpClient { get; private set; }
        protected bool IsDisposed { get; private set; }

        Uri IHttpRequesterConfig.BaseAddress => HttpClient.BaseAddress;
        TimeSpan IHttpRequesterConfig.Timeout => HttpClient.Timeout;
        HttpRequestHeaders IHttpRequesterConfig.Headers => HttpClient.DefaultRequestHeaders;

        public IHttpRequesterConfig Config => this;
        public IJsonSerializer JsonSerializer { get; }

        public static HttpRequester Create(string url, HttpMessageHandler handler = null, IJsonSerializer serializer = null)
            => Create(new Uri(url), handler, serializer);

        public static HttpRequester Create(Uri uri = null, HttpMessageHandler handler = null, IJsonSerializer serializer = null)
            => new HttpRequester(uri, handler, serializer);

        private HttpRequester(Uri uri, HttpMessageHandler handler, IJsonSerializer serializer)
        {
            JsonSerializer = serializer ?? new DefaultJsonSerializer();
            HttpClient = CreateHttpClient(uri, handler);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
            IsDisposed = true;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (IsDisposed || !disposing)
                return;

            if (HttpClient != null)
            {
                HttpClient.CancelPendingRequests();
                HttpClient.Dispose();
                HttpClient = null;
            }
        }

        protected virtual void ThrowIfDisposed()
        {
            if (IsDisposed)
                throw new ObjectDisposedException(GetType().Name);
        }

        protected HttpClient CreateHttpClient(Uri uri = null, HttpMessageHandler handler = null)
        {
            handler = handler ?? CreateDefaultHandler();

            var client = new HttpClient(handler, true);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(HttpContentTypes.Instance.ApplicationJson));

            if (uri != null)
            {
                client.BaseAddress = new Uri(uri.GetAbsoluteAddressExceptUserInfo().TrimEnd('/'));

                var basicAuthString = uri.GetBasicAuthString();
                if (basicAuthString != null)
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", basicAuthString.Value);
            }

            return client;
        }

        private static HttpClientHandler CreateDefaultHandler()
        {
            return new HttpClientHandler
            {
                AllowAutoRedirect = false
            };
        }

        public HttpRequester Configure(Action<IHttpRequesterConfig> config)
        {
            config(this);

            return this;
        }

        IHttpRequesterConfig IHttpRequesterConfig.WithBaseAddress(string value)
        {
            HttpClient.BaseAddress = new Uri(value);

            return this;
        }

        IHttpRequesterConfig IHttpRequesterConfig.WithTimeout(TimeSpan value)
        {
            HttpClient.Timeout = value;

            return this;
        }

        IHttpRequesterConfig IHttpRequesterConfig.WithAccept(Func<HttpContentTypes, string> picker)
        {
            return Config.WithAccept(picker(HttpContentTypes.Instance));
        }

        IHttpRequesterConfig IHttpRequesterConfig.WithAccept(string value)
        {
            return Config.WithHeader(HttpRequesterHeaders.Instance.Accept, value);
        }

        IHttpRequesterConfig IHttpRequesterConfig.WithIfMatch(string value)
        {
            return Config.WithHeader(HttpRequesterHeaders.Instance.IfMatch, value);
        }

        IHttpRequesterConfig IHttpRequesterConfig.WithIfNoneMatch(string value)
        {
            return Config.WithHeader(HttpRequesterHeaders.Instance.IfNoneMatch, value);
        }

        IHttpRequesterConfig IHttpRequesterConfig.WithHeader(Func<HttpRequesterHeaders, string> picker, string value)
        {
            return Config.WithHeader(picker(HttpRequesterHeaders.Instance), value);
        }

        IHttpRequesterConfig IHttpRequesterConfig.WithHeader(string name, string value)
        {
            HttpClient.DefaultRequestHeaders.TryAddWithoutValidation(name, value);

            return this;
        }

        IHttpRequesterConfig IHttpRequesterConfig.WithAuthorization(string value)
        {
            return Config.WithHeader(HttpRequesterHeaders.Instance.Authorization, value);
        }

        IHttpRequesterConfig IHttpRequesterConfig.WithBearer(string value)
        {
            return Config.WithHeader(HttpRequesterHeaders.Instance.Authorization, "Bearer " + value);
        }

        IHttpRequesterConfig IHttpRequesterConfig.WithBasicAuthorization(string username, string password)
        {
            return Config.WithBasicAuthorization(new BasicAuthorizationString(username, password));
        }

        IHttpRequesterConfig IHttpRequesterConfig.WithBasicAuthorization(BasicAuthorizationString value)
        {
            return Config.WithHeader(HttpRequesterHeaders.Instance.Authorization, "Basic " + value);
        }

        public Task<HttpTextResponse> SendAsync(HttpRequest request)
        {
            ThrowIfDisposed();

            return DoSendForTextResponseAsync(request);
        }

        public Task<HttpEntityResponse<TEntity>> SendAsync<TEntity>(HttpRequest request) where TEntity : class
        {
            ThrowIfDisposed();

            return DoSendForEntityResponseAsync<TEntity>(request);
        }

        public Task<HttpTextResponse> DeleteAsync(string relativeUrl = null)
        {
            ThrowIfDisposed();

            var request = new HttpRequest(HttpMethod.Delete, relativeUrl);

            return DoSendForTextResponseAsync(request);
        }

        public Task<HttpTextResponse> HeadAsync(string relativeUrl = null)
        {
            ThrowIfDisposed();

            var request = new HttpRequest(HttpMethod.Head, relativeUrl);

            return DoSendForTextResponseAsync(request);
        }

        public Task<HttpTextResponse> GetAsync(string relativeUrl = null)
        {
            ThrowIfDisposed();

            var request = new HttpRequest(HttpMethod.Get, relativeUrl);

            return DoSendForTextResponseAsync(request);
        }

        public Task<HttpEntityResponse<TEntity>> GetAsync<TEntity>(string relativeUrl = null) where TEntity : class
        {
            ThrowIfDisposed();

            var request = new HttpRequest(HttpMethod.Get, relativeUrl);

            return DoSendForEntityResponseAsync<TEntity>(request);
        }

        public Task<HttpTextResponse> PostAsync(string relativeUrl = null)
        {
            ThrowIfDisposed();

            var request = new HttpRequest(HttpMethod.Post, relativeUrl);

            return DoSendForTextResponseAsync(request);
        }

        public Task<HttpEntityResponse<TEntityOut>> PostAsync<TEntityOut>(string relativeUrl = null) where TEntityOut : class
        {
            ThrowIfDisposed();

            var request = new HttpRequest(HttpMethod.Post, relativeUrl);

            return DoSendForEntityResponseAsync<TEntityOut>(request);
        }

        public Task<HttpTextResponse> PostJsonAsync(string content, string relativeUrl = null)
        {
            ThrowIfDisposed();

            Ensure.That(content, "content").IsNotNullOrWhiteSpace();

            var request = new HttpRequest(HttpMethod.Post, relativeUrl)
                .WithJsonContent(content);

            return DoSendForTextResponseAsync(request);
        }

        public Task<HttpEntityResponse<TEntityOut>> PostJsonAsync<TEntityOut>(string content, string relativeUrl = null) where TEntityOut : class
        {
            ThrowIfDisposed();

            Ensure.That(content, "content").IsNotNullOrWhiteSpace();

            var request = new HttpRequest(HttpMethod.Post, relativeUrl)
                .WithJsonContent(content);

            return DoSendForEntityResponseAsync<TEntityOut>(request);
        }

        public Task<HttpTextResponse> PostEntityAsJsonAsync(object entity, string relativeUrl = null)
        {
            ThrowIfDisposed();

            Ensure.That(entity, "entity").IsNotNull();

            var request = new HttpRequest(HttpMethod.Post, relativeUrl)
                .WithJsonContent(JsonSerializer.Serialize(entity));

            return DoSendForTextResponseAsync(request);
        }

        public Task<HttpEntityResponse<TEntityOut>> PostEntityAsJsonAsync<TEntityOut>(object entity, string relativeUrl = null) where TEntityOut : class
        {
            ThrowIfDisposed();

            Ensure.That(entity, "entity").IsNotNull();

            var request = new HttpRequest(HttpMethod.Post, relativeUrl)
                .WithJsonContent(JsonSerializer.Serialize(entity));

            return DoSendForEntityResponseAsync<TEntityOut>(request);
        }

        public Task<HttpTextResponse> PutAsync(string relativeUrl = null)
        {
            ThrowIfDisposed();

            var request = new HttpRequest(HttpMethod.Put, relativeUrl);

            return DoSendForTextResponseAsync(request);
        }

        public Task<HttpEntityResponse<TEntityOut>> PutAsync<TEntityOut>(string relativeUrl = null) where TEntityOut : class
        {
            ThrowIfDisposed();

            var request = new HttpRequest(HttpMethod.Put, relativeUrl);

            return DoSendForEntityResponseAsync<TEntityOut>(request);
        }

        public Task<HttpTextResponse> PutJsonAsync(string content, string relativeUrl = null)
        {
            ThrowIfDisposed();

            Ensure.That(content, "content").IsNotNullOrWhiteSpace();

            var request = new HttpRequest(HttpMethod.Put, relativeUrl)
                .WithJsonContent(content);

            return DoSendForTextResponseAsync(request);
        }

        public Task<HttpEntityResponse<TEntityOut>> PutJsonAsync<TEntityOut>(string content, string relativeUrl = null) where TEntityOut : class
        {
            ThrowIfDisposed();

            Ensure.That(content, "content").IsNotNullOrWhiteSpace();

            var request = new HttpRequest(HttpMethod.Put, relativeUrl)
                .WithJsonContent(content);

            return DoSendForEntityResponseAsync<TEntityOut>(request);
        }

        public Task<HttpTextResponse> PutEntityAsJsonAsync(object entity, string relativeUrl = null)
        {
            ThrowIfDisposed();

            Ensure.That(entity, "entity").IsNotNull();

            var request = new HttpRequest(HttpMethod.Put, relativeUrl)
                .WithJsonContent(JsonSerializer.Serialize(entity));

            return DoSendForTextResponseAsync(request);
        }

        public Task<HttpEntityResponse<TEntityOut>> PutEntityAsJsonAsync<TEntityOut>(object entity, string relativeUrl = null) where TEntityOut : class
        {
            ThrowIfDisposed();

            Ensure.That(entity, "entity").IsNotNull();

            var request = new HttpRequest(HttpMethod.Put, relativeUrl)
                .WithJsonContent(JsonSerializer.Serialize(entity));

            return DoSendForEntityResponseAsync<TEntityOut>(request);
        }

        private async Task<HttpTextResponse> DoSendForTextResponseAsync(HttpRequest request)
        {
            using (var requestMessage = CreateHttpRequestMessage(request))
                return await CreateHttpTextResponseAsync(requestMessage).ForAwait();
        }

        private async Task<HttpEntityResponse<TEntity>> DoSendForEntityResponseAsync<TEntity>(HttpRequest request) where TEntity : class
        {
            using (var requestMessage = CreateHttpRequestMessage(request))
                return await CreateHttpEntityResponseAsync<TEntity>(requestMessage).ForAwait();
        }

        protected virtual HttpRequestMessage CreateHttpRequestMessage(HttpRequest request)
        {
            var requestMessage = new HttpRequestMessage(request.Method, GenerateRequestUri(request.RelativeUrl));

            foreach (var header in request.Headers)
                requestMessage.Headers.TryAddWithoutValidation(header.Key, header.Value);

            if (request.Content != null)
                requestMessage.Content = request.Content;

            return requestMessage;
        }

        protected virtual Uri GenerateRequestUri(string relativeUrl = null)
        {
            return string.IsNullOrWhiteSpace(relativeUrl)
                ? Config.BaseAddress
                : new Uri($"{Config.BaseAddress.ToString().TrimEnd('/')}/{relativeUrl.TrimStart('/')}".TrimEnd('/'));
        }

        protected virtual Task<HttpTextResponse> CreateHttpTextResponseAsync(HttpRequestMessage request)
        {
            return CreateHttpContentResponseAsync(request, () => new HttpTextResponse(), async msg => await msg.ReadAsStringAsync().ForAwait());
        }

        protected virtual Task<HttpEntityResponse<TEntity>> CreateHttpEntityResponseAsync<TEntity>(HttpRequestMessage request) where TEntity : class
        {
            return CreateHttpContentResponseAsync(request, () => new HttpEntityResponse<TEntity>(), async msg => JsonSerializer.Deserialize<TEntity>(await msg.ReadAsStringAsync().ForAwait()));
        }

        protected virtual async Task<TResponse> CreateHttpContentResponseAsync<TResponse, TContent>(HttpRequestMessage request, Func<TResponse> responseFn, Func<HttpContent, Task<TContent>> contentFn) where TResponse : HttpContentResponse<TContent> where TContent : class
        {
            var response = responseFn();
            response.RequestUri = request.RequestUri;
            response.RequestMethod = request.Method;

            using (var message = await HttpClient.SendAsync(request).ForAwait())
            {
                if (ShouldFollowResponse(message))
                {
                    //TODO: Limit to...
                    request.RequestUri = message.Headers.Location;
                    return await CreateHttpContentResponseAsync(request, responseFn, contentFn).ForAwait();
                }

                response.StatusCode = message.StatusCode;
                response.Reason = message.ReasonPhrase;
                response.Headers = message.Headers?.ToDictionary(h => h.Key, v => v.Value) ??
                                   new Dictionary<string, IEnumerable<string>>();

                if (message.Content?.Headers != null)
                {
                    response.ETag = message.Headers.GetETag();
                    response.Location = message.Headers.GetLocation();
                    response.ContentLength = message.Content.Headers.ContentLength;

                    if (message.Content.Headers.ContentType != null)
                        response.ContentType = message.Content.Headers.ContentType.MediaType;

                    response.Content = await contentFn(message.Content).ForAwait();
                    if ((response.Content as string) == string.Empty)
                        response.Content = null;
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