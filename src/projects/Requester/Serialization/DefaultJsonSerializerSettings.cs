using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

namespace Requester.Serialization
{
    public static class DefaultJsonSerializerSettings
    {
        public static IContractResolver DefaultContractResolver = new DefaultContractResolver
        {
            NamingStrategy = new CamelCaseNamingStrategy(false, false, false)
        };

        public static Func<JsonSerializerSettings, JsonSerializerSettings> Applier { private get; set; } =
            settings =>
            {
                settings.NullValueHandling = NullValueHandling.Ignore;
                settings.DateFormatHandling = DateFormatHandling.IsoDateFormat;
                settings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
                settings.MissingMemberHandling = MissingMemberHandling.Ignore;
                settings.TypeNameHandling = TypeNameHandling.None;

                settings.ContractResolver = DefaultContractResolver;
                settings.Converters.Add(new StringEnumConverter());

                return settings;
            };

        public static JsonSerializerSettings Create() => ApplyTo(new JsonSerializerSettings());

        public static JsonSerializerSettings ApplyTo(JsonSerializerSettings settings) => Applier(settings);
    }
}