using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;

namespace WebRunner
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            AutofacConfig.Configure();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            app.MapSignalR();
        }
    }
}