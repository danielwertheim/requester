using System.Web.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Ninject;
using Ninject.Modules;
using Ninject.Web.Common.OwinHost;
using Ninject.Web.WebApi.OwinHost;
using Owin;
using Requester.TestWebApi.Storage;

namespace Requester.TestWebApi
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var config = new HttpConfiguration();
            config.Formatters.JsonFormatter.SerializerSettings = ConfigureJsonSerializerSettings(config.Formatters.JsonFormatter.SerializerSettings);
            config.MapHttpAttributeRoutes();

            var kernel = CreateKernel();

            app.UseNinjectMiddleware(() => kernel)
               .UseNinjectWebApi(config);
        }

        private static JsonSerializerSettings ConfigureJsonSerializerSettings(JsonSerializerSettings settings)
        {
            settings.NullValueHandling = NullValueHandling.Ignore;
            settings.DateFormatHandling = DateFormatHandling.IsoDateFormat;
            settings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;

            settings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            settings.Converters.Add(new StringEnumConverter());

            return settings;
        }

        private static StandardKernel CreateKernel()
        {
            var kernel = new StandardKernel();
            kernel.Load(typeof(Startup).Assembly);
            return kernel;
        }
    }

    public class Dependencies : NinjectModule
    {
        public override void Load()
        {
            Kernel
                .Bind<IPersonsStore>()
                .To<InMemPersonStore>()
                .InSingletonScope();
        }
    }
}