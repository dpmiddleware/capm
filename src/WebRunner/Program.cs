using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace WebRunner
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration(config =>
                {
                    config
                        .AddInMemoryCollection(new Dictionary<string, string>
                        {
                            ["ComponentsToRun"] = "Archiver;Collector;RandomError"
                        })
                        .AddJsonFile("config.json", optional: true)
                        .AddEnvironmentVariables()
                        .AddCommandLine(args ?? new string[0]);

                    var overrides = new Dictionary<string, string>();
                    if (args != null)
                    {
                        if (args.Contains("--use-storage-emulator"))
                        {
                            Console.WriteLine("Running application using Azure Storage Emulator for Staging Store, AIP Store and Messaging providers");
                            overrides.Add("AzureBlobStorageConnectionString", "UseDevelopmentStorage=true");
                        }
                        if (args.Contains("--use-external-component-runners"))
                        {
                            Console.WriteLine("Running application with only CaPM component. Expecting external component runners to handle other components.");
                            overrides.Add("ComponentsToRun", null);
                        }
                    }
                    if (overrides.Any())
                    {
                        config.AddInMemoryCollection(overrides);
                    }
                })
                .UseServiceProviderFactory(new AutofacServiceProviderFactory())
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
