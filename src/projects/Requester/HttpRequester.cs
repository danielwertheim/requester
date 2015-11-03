using System;
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
    public class HttpRequester : IHttpRequester
    {
        protected HttpClient HttpClient { get; private set; }
        protected bool IsDisposed { get; private set; }
        
        public Uri BaseAddress => HttpClient.BaseAddress;
        public IJsonSerializer JsonSerializer { get; set; }
        public TimeSpan Timeout => HttpClient.Timeout;

        public HttpRequester(Uri uri, HttpMessageHandler handler = null) : this(uri.ToString(), handler) {}

        public HttpRequester(string url, HttpMessageHandler handler = null)
        {
            Ensure.That(url, "url").IsNotNullOrWhiteSpace();

            JsonSerializer = new DefaultJsonSerializer();
            HttpClient = CreateHttpClient(url, handler);
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

        protected HttpClient CreateHttpClient(string url, HttpMessageHandler handler = null)
        {
            var tmpUri = new Uri(url);

            handler = handler ?? CreateDefaultHandler();

            var client = new HttpClient(handler, true)
            {
                BaseAddress = new Uri(tmpUri.GetAbsoluteAddressExceptUserInfo().TrimEnd('/'))
            };
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(HttpContentTypes.Instance.ApplicationJson));

            var basicAuthString = tmpUri.GetBasicAuthString();
            if (basicAuthString != null)
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", basicAuthString.Value);

            return client;
        }

        private static HttpClientHandler CreateDefaultHandler()
        {
            return new HttpClientHandler
            {
                AllowAutoRedirect = false
            };
        }

        public virtual IHttpRequester WithAccept(Func<HttpContentTypes, string> picker)
        {
            return WithAccept(picker(HttpContentTypes.Instance));
        }

        public virtual IHttpRequester WithAccept(string value)
        {
            return WithHeader(HttpRequesterHeaders.Instance.Accept, value);
        }

        public virtual IHttpRequester WithIfMatch(string value)
        {
            return WithHeader(HttpRequesterHeaders.Instance.IfMatch, value);
        }

        public virtual IHttpRequester WithHeader(Func<HttpRequesterHeaders, string> picker, string value)
        {
            return WithHeader(picker(HttpRequesterHeaders.Instance), value);
        }

        public virtual IHttpRequester WithHeader(string name, string value)
        {
            HttpClient.DefaultRequestHeaders.TryAddWithoutValidation(name, value);

            return this;
        }

        public virtual IHttpRequester WithAuthorization(string value)
        {
            return WithHeader(HttpRequesterHeaders.Instance.Authorization, value);
        }

        public virtual IHttpRequester WithBearer(string value)
        {
            return WithHeader(HttpRequesterHeaders.Instance.Authorization, "Bearer " + value);
        }

        public virtual IHttpRequester WithBasicAuthorization(string username, string password)
        {
            return WithBasicAuthorization(new BasicAuthorizationString(username, password));
        }

        public virtual IHttpRequester WithBasicAuthorization(BasicAuthorizationString value)
        {
            return WithHeader(HttpRequesterHeaders.Instance.Authorization, "Basic " + value);
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

        public Task<HttpTextResponse> PostEntityAsync(object entity, string relativeUrl = null)
        {
            ThrowIfDisposed();

            Ensure.That(entity, "entity").IsNotNull();

            var request = new HttpRequest(HttpMethod.Post, relativeUrl)
                .WithJsonContent(JsonSerializer.Serialize(entity));

            return DoSendForTextResponseAsync(request);
        }

        public Task<HttpEntityResponse<TEntityOut>> PostEntityAsync<TEntityOut>(object entity, string relativeUrl = null) where TEntityOut : class
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

        public Task<HttpTextResponse> PutEntityAsync(object entity, string relativeUrl = null)
        {
            ThrowIfDisposed();

            Ensure.That(entity, "entity").IsNotNull();

            var request = new HttpRequest(HttpMethod.Put, relativeUrl)
                .WithJsonContent(JsonSerializer.Serialize(entity));

            return DoSendForTextResponseAsync(request);
        }

        public Task<HttpEntityResponse<TEntityOut>> PutEntityAsync<TEntityOut>(object entity, string relativeUrl = null) where TEntityOut : class
        {
            ThrowIfDisposed();

            Ensure.That(entity, "entity").IsNotNull();

            var request = new HttpRequest(HttpMethod.Put, relativeUrl)
                .WithJsonContent(JsonSerializer.Serialize(entity));

            return DoSendForEntityResponseAsync<TEntityOut>(request);
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

            if(request.Content != null)
                requestMessage.Content = request.Content;

            return requestMessage;
        }

        protected virtual Uri GenerateRequestUri(string relativeUrl = null)
        {
            return string.IsNullOrWhiteSpace(relativeUrl)
                ? BaseAddress
                : new Uri($"{BaseAddress.ToString().TrimEnd('/')}/{relativeUrl.TrimStart('/')}".TrimEnd('/'));
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