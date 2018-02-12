using Autofac;
using Autofac.Integration.WebApi;
using ComponentRunnerHelpers;
using Microsoft.AspNet.SignalR;
using Microsoft.WindowsAzure.Storage;
using Newtonsoft.Json;
using PoF.CaPM;
using PoF.CaPM.SubmissionAgreements;
using PoF.Common;
using PoF.Components.Archiver;
using PoF.Components.Collector;
using PoF.Components.RandomError;
using PoF.FakeImplementations;
using PoF.Messaging;
using PoF.Messaging.InMemory;
using PoF.Messaging.ServiceBus;
using PoF.StagingStore;
using PoF.StagingStore.Azure;
using PoF.StagingStore.InMemory;
using System;
using System.Linq;
using System.Configuration;
using System.IO;
using System.Reflection;
using WebRunner.Controllers;
using WebRunner.Services;
using System.Collections.Generic;

namespace WebRunner
{
    internal static class AutofacConfig
    {
        public static void Configure()
        {
            ValidateComponentsToRunConfiguration();
            IContainer container = BootstrapIoCContainer();
            foreach(var componentName in ComponentsToRun)
            {
                StartComponent(container, _moduleTypeDictionary[componentName]);
            }
            container.Resolve<CaPMSystem>().Start();

            var webApiResolver = new AutofacWebApiDependencyResolver(container);
            System.Web.Http.GlobalConfiguration.Configuration.DependencyResolver = webApiResolver;
            GlobalHost.DependencyResolver.Register(typeof(IngestEventsHub), () => container.Resolve<IngestEventsHub>());
            GlobalHost.DependencyResolver.Register(typeof(PreservationSystemHub), () => container.Resolve<PreservationSystemHub>());
        }

        private static void StartComponent(IContainer container, Type componentType)
        {
            var component = (IComponent)container.Resolve(componentType);
            component.Start();
        }

        private static IContainer BootstrapIoCContainer()
        {
            return ComponentRunnerHelper.BootstrapIoCContainer(builder =>
            {
                builder.RegisterApiControllers(Assembly.GetExecutingAssembly());
                builder.Register(context => CreateSubmissionAgreementStore()).As<ISubmissionAgreementStore>().SingleInstance();
                ComponentRunnerHelper.AddComponentModule<CaPMAutofacModule>(builder);
                LoadComponents(builder);
                builder.RegisterType<CaPMSystem>().SingleInstance();
                builder.RegisterType<CaPMEventStore>().As<ICaPMEventStore>().SingleInstance();
                ConfigureAipStore(builder);
                builder.RegisterType<IngestEventsHub>().InstancePerDependency();
                builder.RegisterType<PreservationSystemHub>().InstancePerDependency();
            });
        }

        private static void LoadComponents(ContainerBuilder builder)
        {
            foreach (var componentName in ComponentsToRun)
            {
                var componentModule = (Autofac.Core.IModule)Activator.CreateInstance(_moduleTypeDictionary[componentName].Assembly.GetType($"PoF.Components.{componentName}.{componentName}AutofacModule"));
                builder.RegisterModule(componentModule);
            }
        }

        private static Dictionary<string, Type> _moduleTypeDictionary = new Dictionary<string, Type>()
        {
            ["Archiver"] = typeof(ArchiverComponent),
            ["Collector"] = typeof(CollectorComponent),
            ["RandomError"] = typeof(RandomErrorComponent)
        };

        private static void ValidateComponentsToRunConfiguration()
        {
            foreach (var componentName in ComponentsToRun)
            {
                if (!_moduleTypeDictionary.ContainsKey(componentName))
                {
                    throw new NotSupportedException($"Application configuration invalid. Component name '{componentName}' is not a supported component. Valid component names are: {string.Join(", ", _moduleTypeDictionary.Keys)}");
                }
            }
        }

        private static string[] ComponentsToRun
        {
            get
            {
                return ConfigurationManager.AppSettings["ComponentsToRun"]?.Split(',', ';').Select(c => c.Trim()).ToArray() ?? new string[0];
            }
        }

        private static string AzureStorageConnectionString => ConfigurationManager.ConnectionStrings["AzureBlobStorageStagingStoreConnectionString"]?.ConnectionString;

        private static void ConfigureAipStore(ContainerBuilder builder)
        {
            IAipStore store;
            //TODO: Should use its own storage connection string instead of using the Staging Store connection string
            var connectionString = AzureStorageConnectionString;
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                store = new InMemoryAipStore();
            }
            else
            {
                var blobAipStore = new AzureBlobStorageAipStore(connectionString);
                store = blobAipStore;
                blobAipStore.Initialize().Wait();
            }
            builder.Register(context => store).As<IAipStore>().SingleInstance();
        }

        private static ISubmissionAgreementStore CreateSubmissionAgreementStore()
        {
            if ("DEV".Equals(Environment.GetEnvironmentVariable("ENVIRONMENT"), StringComparison.CurrentCultureIgnoreCase))
            {
                return new FakeSubmissionAgreementStore();
            }
            else
            {
                var store = new InMemorySubmissionAgreementStore();
                var agreementsString = File.ReadAllText(Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "SubmissionAgreements.json"));
                var agreements = JsonConvert.DeserializeObject<SubmissionAgreement[]>(agreementsString);
                foreach (var agreement in agreements)
                {
                    store.Add(agreement);
                }
                return store;
            }
        }
    }
}