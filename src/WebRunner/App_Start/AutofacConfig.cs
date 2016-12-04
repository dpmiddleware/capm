using Autofac;
using Autofac.Integration.WebApi;
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
using PoF.StagingStore;
using PoF.StagingStore.Azure;
using PoF.StagingStore.InMemory;
using System;
using System.Configuration;
using System.IO;
using System.Reflection;
using WebRunner.Controllers;

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
        }

        private static void StartComponent<T>(IContainer container)
            where T : IComponent
        {
            IComponent component = container.Resolve<T>();
            component.Start();
        }

        private static IContainer BootstrapIoCContainer()
        {
            var builder = new ContainerBuilder();
            builder.RegisterApiControllers(Assembly.GetExecutingAssembly());
            IContainer container = null;

            builder.RegisterModule<CaPMAutofacModule>();
            builder.RegisterModule<CollectorAutofacModule>();
            builder.RegisterModule<ArchiverAutofacModule>();
            builder.RegisterModule<RandomErrorAutofacModule>();

            builder.RegisterType<FakeComponentChannelIdentifierRepository>().As<IComponentChannelIdentifierRepository>().SingleInstance();
            builder.Register(context => CreateSubmissionAgreementStore()).As<ISubmissionAgreementStore>().SingleInstance();
            builder.RegisterType<CaPMSystem>().SingleInstance();
            builder.RegisterType<CaPMEventStore>().As<ICaPMEventStore>().SingleInstance();
            builder.RegisterType<CollectorComponent>().SingleInstance();
            builder.RegisterType<ArchiverComponent>().SingleInstance();
            builder.RegisterType<RandomErrorComponent>().SingleInstance();
            builder.RegisterType<CommandMessageListener>().As<ICommandMessageListener>().InstancePerDependency();
            builder.RegisterType<InMemoryMessageSenderFactory>().As<IMessageSenderFactory>().SingleInstance();
            builder.RegisterType<InMemoryMessageSource>().As<IMessageSource>().SingleInstance();
            ConfigureAzureBlobStorageStagingStore(builder);
            builder.Register(context => container).As<IContainer>().SingleInstance();

            builder.RegisterType<IngestEventsHub>().InstancePerDependency();

            container = builder.Build();
            return container;
        }

        private static void ConfigureInMemoryStagingStore(ContainerBuilder builder)
        {
            builder.RegisterType<InMemoryStagingStoreContainer>().As<IStagingStoreContainer>().SingleInstance();
        }

        private static void ConfigureAzureBlobStorageStagingStore(ContainerBuilder builder)
        {
            var connectionString = ConfigurationManager.ConnectionStrings["AzureBlobStorageStagingStoreConnectionString"].ConnectionString;
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new Exception("Invalid configuration. Missing connection string with name 'AzureBlobStorageStagingStoreConnectionString'.");
            }
            var storageAccount = CloudStorageAccount.Parse(connectionString);
            var cloudBlobClient = storageAccount.CreateCloudBlobClient();
            builder.Register(context => new AzureBlobStorageStagingStoreContainer(cloudBlobClient)).As<IStagingStoreContainer>();
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