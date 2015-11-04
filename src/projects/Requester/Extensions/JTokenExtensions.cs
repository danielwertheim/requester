using Newtonsoft.Json.Linq;

namespace Requester.Extensions
{
    internal static class JTokenExtensions
    {
        internal static bool ValueIsEqualTo<T>(this JToken node, T expected)
        {
            return node.Equals(JToken.FromObject(expected));
        }
    }
}