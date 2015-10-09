using System.Threading.Tasks;

namespace Requester
{
    public interface IHttpRequester
    {
        IJsonSerializer JsonSerializer { get; set; }

        Task<HttpTextResponse> SendAsync(HttpRequest request);
        Task<HttpEntityResponse<TEntity>> SendAsync<TEntity>(HttpRequest request) where TEntity : class;

        Task<HttpTextResponse> DeleteAsync(string relativeUrl = null);

        Task<HttpTextResponse> HeadAsync(string relativeUrl = null);

        Task<HttpTextResponse> GetAsync(string relativeUrl = null);
        Task<HttpEntityResponse<TEntity>> GetAsync<TEntity>(string relativeUrl = null) where TEntity : class;

        Task<HttpTextResponse> PostAsync(string relativeUrl = null);
        Task<HttpTextResponse> PostContentAsync(string content, string relativeUrl = null);
        Task<HttpTextResponse> PostContentAsync<TEntity>(TEntity entity, string relativeUrl = null) where TEntity : class;
        Task<HttpEntityResponse<TEntityOut>> PostContentAsync<TEntityIn, TEntityOut>(TEntityIn entity, string relativeUrl = null) where TEntityIn : class where TEntityOut : class;

        Task<HttpTextResponse> PutAsync(string relativeUrl = null);
        Task<HttpTextResponse> PutContentAsync(string content, string relativeUrl = null);
        Task<HttpTextResponse> PutContentAsync<TEntity>(TEntity entity, string relativeUrl = null) where TEntity : class;
        Task<HttpEntityResponse<TEntityOut>> PutContentAsync<TEntityIn, TEntityOut>(TEntityIn entity, string relativeUrl = null) where TEntityIn : class where TEntityOut : class;
    }
}