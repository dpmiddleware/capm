using System;
using System.Collections.Generic;
using System.IO;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using PoF.CaPM;
using PoF.Common;
using PoF.Messaging;
using WebRunner.Controllers;

namespace WebRunner
{
    public class Startup
    {
        private readonly IConfiguration Configuration;

        public Startup(IConfiguration configuration)
        {
            this.Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.Converters.Add(new InterfaceToConcreteTypeConverter<PoF.CaPM.IngestSaga.Events.IIngestEvent>());
                });
            services.AddControllers();
            services.AddSignalR();
        }

        private static void StartComponent(ILifetimeScope lifetimeScope, Type componentType)
        {
            var component = (IComponent)lifetimeScope.Resolve(componentType);
            component.Start();
        }

        public void ConfigureContainer(ContainerBuilder builder)
        {
            AutofacConfig.BootstrapIoCContainer(builder, Configuration);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            var lifetimeScope = app.ApplicationServices.GetAutofacRoot();
            foreach (var componentName in AutofacConfig.GetComponentsToRun(Configuration))
            {
                StartComponent(lifetimeScope, AutofacConfig.ModuleTypeDictionary[componentName]);
            }
            lifetimeScope.Resolve<CaPMSystem>().Start();
            Controllers.IngestEventsHub.Start(lifetimeScope.Resolve<IChannelProvider>(), lifetimeScope.Resolve<IComponentChannelIdentifierRepository>(), lifetimeScope.Resolve<IHubContext<IngestEventsHub>>());
            Controllers.PreservationSystemHub.Initialize(lifetimeScope.Resolve<IHubContext<PreservationSystemHub>>());

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHttpsRedirection();
            }

            var publicFilesFileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "public"));
            app.UseDefaultFiles(new DefaultFilesOptions()
            {
                DefaultFileNames = new List<string> { "index.html" },
                RequestPath = null,
                FileProvider = publicFilesFileProvider
            });

            app.UseStaticFiles(new StaticFileOptions()
            {
                RequestPath = null,
                FileProvider = publicFilesFileProvider
            });

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHub<IngestEventsHub>("/hubs/ingest");
                endpoints.MapHub<PreservationSystemHub>("/hubs/dps");
            });
        }
    }
}
