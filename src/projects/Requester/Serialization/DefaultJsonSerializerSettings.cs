using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace Requester.Serialization
{
    public static class DefaultJsonSerializerSettings
    {
        public static IContractResolver DefaultContractResolver = new CamelCasePropertyNamesContractResolver();

        public static JsonSerializerSettings Create()
        {
            return ApplyTo(new JsonSerializerSettings());
        }

        public static JsonSerializerSettings ApplyTo(JsonSerializerSettings settings)
        {
            settings.NullValueHandling = NullValueHandling.Ignore;
            settings.DateFormatHandling = DateFormatHandling.IsoDateFormat;
            settings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;

            settings.ContractResolver = DefaultContractResolver;
            settings.Converters.Add(new StringEnumConverter());

            return settings;
        }
    }
}