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
    public class HttpRequester : IHttpRequester, IDisposable
    {
        protected HttpClient HttpClient { get; private set; }
        protected bool IsDisposed { get; private set; }
        
        public Uri BaseAddress => HttpClient.BaseAddress;
        public IJsonSerializer JsonSerializer { get; set; }
        public TimeSpan Timeout => HttpClient.Timeout;

        public HttpRequester(string url)
        {
            Ensure.That(url, "uri").IsNotNullOrWhiteSpace();

            JsonSerializer = new DefaultJsonSerializer();
            HttpClient = CreateHttpClient(url);
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

        protected HttpClient CreateHttpClient(string url)
        {
            var tmpUri = new Uri(url);

            var handler = new HttpClientHandler
            {
                AllowAutoRedirect = false
            };

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

        public Task<HttpTextResponse> PostContentAsync(string content, string relativeUrl = null)
        {
            ThrowIfDisposed();

            Ensure.That(content, "content").IsNotNullOrWhiteSpace();

            var request = new HttpRequest(HttpMethod.Post, relativeUrl)
                .WithJsonContent(content);

            return DoSendForTextResponseAsync(request);
        }

        public Task<HttpTextResponse> PostContentAsync<TEntity>(TEntity entity, string relativeUrl = null) where TEntity : class
        {
            ThrowIfDisposed();

            Ensure.That(entity, "entity").IsNotNull();

            var request = new HttpRequest(HttpMethod.Post, relativeUrl)
                .WithJsonContent(JsonSerializer.Serialize(entity));

            return DoSendForTextResponseAsync(request);
        }

        public Task<HttpEntityResponse<TEntityOut>> PostContentAsync<TEntityIn, TEntityOut>(TEntityIn entity, string relativeUrl = null) where TEntityIn : class where TEntityOut : class
        {
            ThrowIfDisposed();

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

        public Task<HttpTextResponse> PutContentAsync(string content, string relativeUrl = null)
        {
            ThrowIfDisposed();

            Ensure.That(content, "content").IsNotNullOrWhiteSpace();

            var request = new HttpRequest(HttpMethod.Put, relativeUrl)
                .WithJsonContent(content);

            return DoSendForTextResponseAsync(request);
        }

        public Task<HttpTextResponse> PutContentAsync<TEntity>(TEntity entity, string relativeUrl = null) where TEntity : class
        {
            ThrowIfDisposed();

            Ensure.That(entity, "entity").IsNotNull();

            var request = new HttpRequest(HttpMethod.Put, relativeUrl)
                .WithJsonContent(JsonSerializer.Serialize(entity));

            return DoSendForTextResponseAsync(request);
        }

        public Task<HttpEntityResponse<TEntityOut>> PutContentAsync<TEntityIn, TEntityOut>(TEntityIn entity, string relativeUrl = null) where TEntityIn : class where TEntityOut : class
        {
            ThrowIfDisposed();

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
            return CreateHttpContentResponseAsync(request, () => new HttpTextResponse(), async msg => await msg.ReadAsStringAsync());
        }

        protected virtual Task<HttpEntityResponse<TEntity>> CreateHttpEntityResponseAsync<TEntity>(HttpRequestMessage request) where TEntity : class
        {
            return CreateHttpContentResponseAsync(request, () => new HttpEntityResponse<TEntity>(), async msg => JsonSerializer.Deserialize<TEntity>(await msg.ReadAsStringAsync()));
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
                    response.ContentLength = message.Content.Headers.ContentLength;

                    if (message.Content.Headers.ContentType != null)
                        response.ContentType = message.Content.Headers.ContentType.MediaType;

                    response.Content = await contentFn(message.Content).ForAwait(); //await message.Content.ReadAsStringAsync().ForAwait();
                    //if (response.Content == string.Empty)
                    //    response.Content = null;
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