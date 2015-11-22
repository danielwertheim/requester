using System;
using System.Net.Http.Headers;
using Requester.Http;

namespace Requester
{
    public interface IHttpRequesterConfig
    {
        Uri BaseAddress { get; }
        TimeSpan Timeout { get; }
        HttpRequestHeaders Headers { get; }

        IHttpRequesterConfig WithBaseAddress(string value);
        IHttpRequesterConfig WithTimeout(TimeSpan value);
        IHttpRequesterConfig WithAccept(Func<HttpContentTypes, string> picker);
        IHttpRequesterConfig WithAccept(string value);
        IHttpRequesterConfig WithIfMatch(string value);
        IHttpRequesterConfig WithHeader(Func<HttpRequesterHeaders, string> picker, string value);
        IHttpRequesterConfig WithHeader(string name, string value);
        IHttpRequesterConfig WithAuthorization(string value);
        IHttpRequesterConfig WithBearer(string value);
        IHttpRequesterConfig WithBasicAuthorization(string username, string password);
        IHttpRequesterConfig WithBasicAuthorization(BasicAuthorizationString value);
    }
}