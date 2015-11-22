using Newtonsoft.Json.Linq;

namespace Requester.Extensions
{
    public static class JTokenExtensions
    {
        public static bool ValueIsEqualTo<T>(this JToken node, T expected)
        {
            return node.Equals(JToken.FromObject(expected));
        }
    }
}