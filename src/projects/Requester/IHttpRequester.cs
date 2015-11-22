using System.Threading.Tasks;

namespace Requester
{
    public interface IHttpRequester
    {
        IHttpRequesterConfig Config { get; }
        IJsonSerializer JsonSerializer { get; }

        Task<HttpTextResponse> SendAsync(HttpRequest request);
        Task<HttpEntityResponse<TEntity>> SendAsync<TEntity>(HttpRequest request) where TEntity : class;

        Task<HttpTextResponse> DeleteAsync(string relativeUrl = null);
        Task<HttpTextResponse> HeadAsync(string relativeUrl = null);
        Task<HttpTextResponse> GetAsync(string relativeUrl = null);
        Task<HttpEntityResponse<TEntity>> GetAsync<TEntity>(string relativeUrl = null) where TEntity : class;

        Task<HttpTextResponse> PostAsync(string relativeUrl = null);
        Task<HttpEntityResponse<TEntityOut>> PostAsync<TEntityOut>(string relativeUrl = null) where TEntityOut : class;
        Task<HttpTextResponse> PostJsonAsync(string content, string relativeUrl = null);
        Task<HttpEntityResponse<TEntityOut>> PostJsonAsync<TEntityOut>(string content, string relativeUrl = null) where TEntityOut : class;
        Task<HttpTextResponse> PostEntityAsJsonAsync(object entity, string relativeUrl = null);
        Task<HttpEntityResponse<TEntityOut>> PostEntityAsJsonAsync<TEntityOut>(object entity, string relativeUrl = null) where TEntityOut : class;

        Task<HttpTextResponse> PutAsync(string relativeUrl = null);
        Task<HttpEntityResponse<TEntityOut>> PutAsync<TEntityOut>(string relativeUrl = null) where TEntityOut : class;
        Task<HttpTextResponse> PutJsonAsync(string content, string relativeUrl = null);
        Task<HttpEntityResponse<TEntityOut>> PutJsonAsync<TEntityOut>(string content, string relativeUrl = null) where TEntityOut : class;
        Task<HttpTextResponse> PutEntityAsJsonAsync(object entity, string relativeUrl = null);
        Task<HttpEntityResponse<TEntityOut>> PutEntityAsJsonAsync<TEntityOut>(object entity, string relativeUrl = null) where TEntityOut : class;
    }
}