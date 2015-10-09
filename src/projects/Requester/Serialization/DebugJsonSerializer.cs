using Newtonsoft.Json;

namespace Requester.Serialization
{
    internal static class DebugJsonSerializer
    {
        internal static readonly IJsonSerializer Instance = new DefaultJsonSerializer(s => s.Formatting = Formatting.Indented);
    }
}