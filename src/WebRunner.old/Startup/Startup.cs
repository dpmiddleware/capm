using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(WebRunner.Startup.Startup))]

namespace WebRunner.Startup
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            AutofacStartup.Configuration(app);
            WebApiStartup.Configuration(app);
        }
    }
}
