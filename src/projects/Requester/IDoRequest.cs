using System;
using System.Threading.Tasks;

namespace Requester
{
    public interface IDoRequest
    {
        IDoRequest WithAccept(Func<ContentTypes, string> picker);
        IDoRequest WithAccept(string value);
        IDoRequest WithIfMatch(string value);
        IDoRequest WithHeader(Func<HttpHeaders, string> picker, string value);
        IDoRequest WithHeader(string name, string value);
        IDoRequest WithBasicAuthorization(string username, string password);
        IDoRequest WithBasicAuthorization(BasicAuthorizationString value);
        IDoRequest WithJsonContent(string content);

        Task<HttpResponse> UsingHeadAsync(string relativeUrl = null);
        Task<HttpResponse> UsingGetAsync(string relativeUrl = null);
        Task<HttpResponse> UsingPostAsync(string relativeUrl = null);
        Task<HttpResponse> UsingPutAsync(string relativeUrl = null);
        Task<HttpResponse> UsingDeleteAsync(string relativeUrl = null);
    }
}