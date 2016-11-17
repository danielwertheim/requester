using System;
using Newtonsoft.Json;

namespace Requester.Serialization
{
    public class DefaultJsonSerializer : IJsonSerializer
    {
        public JsonSerializerSettings Settings { get; set; }

        public DefaultJsonSerializer(Action<JsonSerializerSettings> settingsCfg = null)
        {
            Settings = DefaultJsonSerializerSettings.Create();

            settingsCfg?.Invoke(Settings);
        }

        public virtual string Serialize<T>(T item)
        {
            return JsonConvert.SerializeObject(item, Settings);
        }

        public virtual T Deserialize<T>(string json) where T : class
        {
            return string.IsNullOrWhiteSpace(json)
                ? null
                : JsonConvert.DeserializeObject<T>(json, Settings);
        }
    }
}