using System.Web.Http;
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
            config.MapHttpAttributeRoutes();

            var kernel = CreateKernel();

            app.UseNinjectMiddleware(() => kernel)
               .UseNinjectWebApi(config);
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