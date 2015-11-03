using System;
using System.Threading.Tasks;
using Requester.Http;

namespace Requester
{
    public interface IHttpRequester : IDisposable
    {
        Uri BaseAddress { get; }
        IJsonSerializer JsonSerializer { get; set; }
        TimeSpan Timeout { get; }

        IHttpRequester WithAccept(Func<HttpContentTypes, string> picker);
        IHttpRequester WithAccept(string value);
        IHttpRequester WithIfMatch(string value);
        IHttpRequester WithHeader(Func<HttpRequesterHeaders, string> picker, string value);
        IHttpRequester WithHeader(string name, string value);
        IHttpRequester WithAuthorization(string value);
        IHttpRequester WithBearer(string value);
        IHttpRequester WithBasicAuthorization(string username, string password);
        IHttpRequester WithBasicAuthorization(BasicAuthorizationString value);
        Task<HttpTextResponse> DeleteAsync(string relativeUrl = null);
        Task<HttpTextResponse> HeadAsync(string relativeUrl = null);
        Task<HttpTextResponse> GetAsync(string relativeUrl = null);
        Task<HttpEntityResponse<TEntity>> GetAsync<TEntity>(string relativeUrl = null) where TEntity : class;
        Task<HttpTextResponse> PostAsync(string relativeUrl = null);
        Task<HttpTextResponse> PostJsonAsync(string content, string relativeUrl = null);
        Task<HttpTextResponse> PostEntityAsync(object entity, string relativeUrl = null);
        Task<HttpEntityResponse<TEntityOut>> PostEntityAsync<TEntityOut>(object entity, string relativeUrl = null) where TEntityOut : class;
        Task<HttpTextResponse> PutAsync(string relativeUrl = null);
        Task<HttpTextResponse> PutJsonAsync(string content, string relativeUrl = null);
        Task<HttpTextResponse> PutEntityAsync(object entity, string relativeUrl = null);
        Task<HttpEntityResponse<TEntityOut>> PutEntityAsync<TEntityOut>(object entity, string relativeUrl = null) where TEntityOut : class;
        Task<HttpTextResponse> SendAsync(HttpRequest request);
        Task<HttpEntityResponse<TEntity>> SendAsync<TEntity>(HttpRequest request) where TEntity : class;
    }
}