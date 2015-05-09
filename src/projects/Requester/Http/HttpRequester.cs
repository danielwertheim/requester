using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using EnsureThat;
using Requester.Extensions;

namespace Requester.Http
{
    public class HttpRequester : IHttpRequester, IDisposable
    {
        protected HttpClient HttpClient { get; private set; }
        protected bool IsDisposed { get; private set; }

        public Uri BaseAddress
        {
            get { return HttpClient.BaseAddress; }
        }

        public TimeSpan Timeout
        {
            get { return HttpClient.Timeout; }
            set { HttpClient.Timeout = value; }
        }
 
        public HttpRequester(string url)
        {
            Ensure.That(url, "uri").IsNotNullOrWhiteSpace();

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
                BaseAddress = new Uri(tmpUri.GetAbsoluteAddressExceptUserInfo().TrimEnd(new[] { '/' }))
            };
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(HttpContentTypes.Instance.ApplicationJson));

            var basicAuthString = tmpUri.GetBasicAuthString();
            if (basicAuthString != null)
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", basicAuthString.Value);
            }

            return client;
        }

        public virtual async Task<HttpResponse> SendAsync(HttpRequest request)
        {
            ThrowIfDisposed();

            using (var requestMessage = CreateHttpRequestMessage(request))
                return await CreateHttpResponseAsync(requestMessage).ForAwait();
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
                : new Uri(string.Format("{0}/{1}", BaseAddress.ToString().TrimEnd('/'), relativeUrl.TrimStart('/')).TrimEnd('/'));
        }

        protected virtual async Task<HttpResponse> CreateHttpResponseAsync(HttpRequestMessage request)
        {
            var response = new HttpResponse
            {
                RequestUri = request.RequestUri,
                RequestMethod = request.Method
            };

            using (var message = await HttpClient.SendAsync(request).ForAwait())
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

                        if (message.Content.Headers.ContentType != null)
                            response.ContentType = message.Content.Headers.ContentType.MediaType;

                        response.Content = await message.Content.ReadAsStringAsync().ForAwait();
                        if (response.Content == string.Empty)
                            response.Content = null;
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