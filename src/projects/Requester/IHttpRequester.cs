using System.Threading.Tasks;
using Requester.Http;

namespace Requester
{
    public interface IHttpRequester
    {
        Task<HttpResponse> SendAsync(HttpRequest request);
    }
}