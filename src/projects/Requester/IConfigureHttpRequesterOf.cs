using System;

namespace Requester
{
    public interface IConfigureHttpRequesterOf<out T> where T : IHttpRequester
    {
        T Configure(Action<IHttpRequesterConfig> config);
    }
}