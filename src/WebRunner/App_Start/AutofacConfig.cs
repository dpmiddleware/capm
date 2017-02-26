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
using System.Configuration;
using System.IO;
using System.Reflection;
using WebRunner.Controllers;
using WebRunner.Services;

namespace WebRunner
{
    internal static class AutofacConfig
    {
        public static void Configure()
        {
            IContainer container = BootstrapIoCContainer();
            StartComponent<CollectorComponent>(container);
            StartComponent<RandomErrorComponent>(container);
            StartComponent<ArchiverComponent>(container);
            StartComponent<CaPMSystem>(container);

            var webApiResolver = new AutofacWebApiDependencyResolver(container);
            System.Web.Http.GlobalConfiguration.Configuration.DependencyResolver = webApiResolver;
            GlobalHost.DependencyResolver.Register(typeof(IngestEventsHub), () => container.Resolve<IngestEventsHub>());
            GlobalHost.DependencyResolver.Register(typeof(PreservationSystemHub), () => container.Resolve<PreservationSystemHub>());
        }

        private static void StartComponent<T>(IContainer container)
            where T : IComponent
        {
            IComponent component = container.Resolve<T>();
            component.Start();
        }

        private static IContainer BootstrapIoCContainer()
        {
            return ComponentRunnerHelper.BootstrapIoCContainer(builder =>
            {
                builder.RegisterApiControllers(Assembly.GetExecutingAssembly());
                builder.Register(context => CreateSubmissionAgreementStore()).As<ISubmissionAgreementStore>().SingleInstance();
                ComponentRunnerHelper.AddComponentModule<CaPMAutofacModule>(builder);
                ComponentRunnerHelper.AddComponentModule<CollectorAutofacModule>(builder);
                ComponentRunnerHelper.AddComponentModule<RandomErrorAutofacModule>(builder);
                ComponentRunnerHelper.AddComponentModule<ArchiverAutofacModule>(builder);
                builder.RegisterType<CaPMSystem>().SingleInstance();
                builder.RegisterType<CaPMEventStore>().As<ICaPMEventStore>().SingleInstance();
                ConfigureAipStore(builder);
                builder.RegisterType<IngestEventsHub>().InstancePerDependency();
                builder.RegisterType<PreservationSystemHub>().InstancePerDependency();
            });
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
            var store = new InMemorySubmissionAgreementStore();
            var agreementsString = File.ReadAllText(Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "SubmissionAgreements.json"));
            var agreements = JsonConvert.DeserializeObject<SubmissionAgreement[]>(agreementsString);
            foreach(var agreement in agreements)
            {
                store.Add(agreement);
            }
            return store;
        }
    }
}