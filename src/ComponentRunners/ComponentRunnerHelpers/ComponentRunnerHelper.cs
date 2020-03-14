using Autofac;
using Autofac.Core;
using Microsoft.WindowsAzure.Storage;
using PoF.Common;
using PoF.FakeImplementations;
using PoF.Messaging;
using PoF.Messaging.InMemory;
using PoF.Messaging.ServiceBus;
using PoF.StagingStore;
using PoF.StagingStore.Azure;
using PoF.StagingStore.Filesystem;
using PoF.StagingStore.InMemory;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace ComponentRunnerHelpers
{
    public class ComponentRunnerHelper
    {
        public static void AddComponentModule<ComponentAutofacModuleType>(ContainerBuilder builder)
            where ComponentAutofacModuleType : IModule, new()
        {
            builder.RegisterModule<ComponentAutofacModuleType>();
        }

        public static IConfiguration GetConfiguration(string[] commandLineArgs)
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile("config.json", optional: true)
                .AddEnvironmentVariables()
                .AddCommandLine(commandLineArgs ?? new string[0]);

            IConfiguration configuration = builder.Build();
            return configuration;
        }

        public static void BootstrapIoCContainer(ContainerBuilder builder, IConfiguration configuration)
        {
            builder.RegisterType<FakeComponentChannelIdentifierRepository>().As<IComponentChannelIdentifierRepository>().SingleInstance();
            builder.RegisterType<CommandMessageListener>().As<ICommandMessageListener>().InstancePerDependency();
            ConfigureMessageSender(configuration, builder);
            ConfigureStagingStore(configuration, builder);
        }

        public static IContainer BootstrapIoCContainer(IConfiguration configuration, params Action<ContainerBuilder>[] additionalBootstrappers)
        {
            var builder = new ContainerBuilder();
            if (additionalBootstrappers != null)
            {
                foreach (var bootstrappingAction in additionalBootstrappers)
                {
                    bootstrappingAction(builder);
                }
            }
            BootstrapIoCContainer(builder, configuration);
            IContainer container = null;
            builder.Register(context => container).As<IContainer>().SingleInstance();
            container = builder.Build();
            return container;
        }

        private static void ConfigureMessageSender(IConfiguration configuration, ContainerBuilder builder)
        {
            //TODO: Should use its own connection string instead of the staging store connection string
            var connectionString = configuration[AzureStorageConnectionString_ConfigurationKey];
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                builder.Register(_ => InMemoryMessageChannelProvider.Instance).As<InMemoryMessageChannelProvider>().As<IChannelProvider>().SingleInstance();
                builder.RegisterType<InMemoryMessageSenderFactory>().As<IMessageSenderFactory>().SingleInstance();
#if DEBUG
                Console.WriteLine("Configuring in-memory message provider");
#endif
            }
            else
            {
                builder.Register(_ => new ServiceBusMessageChannelProvider(connectionString)).As<ServiceBusMessageChannelProvider>().As<IChannelProvider>().SingleInstance();
                builder.RegisterType<ServiceBusMessageSenderFactory>().As<IMessageSenderFactory>().SingleInstance();
#if DEBUG
                Console.WriteLine("Configuring azure storage queue message provider");
#endif
            }
        }

        private static void ConfigureStagingStore(IConfiguration configuration, ContainerBuilder builder)
        {
            var connectionstring = configuration[AzureStorageConnectionString_ConfigurationKey];
            if (string.IsNullOrWhiteSpace(connectionstring))
            {
                ConfigureInMemoryStagingStore(builder);
#if DEBUG
                Console.WriteLine("Configuring in-memory staging store provider");
#endif
            }
            else
            {
                ConfigureAzureBlobStorageStagingStore(connectionstring, builder);
#if DEBUG
                Console.WriteLine("Configuring azure storage blobs staging store provider");
#endif
            }
        }

        private static void ConfigureInMemoryStagingStore(ContainerBuilder builder)
        {
            builder.RegisterType<InMemoryStagingStoreContainer>().As<IStagingStoreContainer>().SingleInstance();
        }

        private const string AzureStorageConnectionString_ConfigurationKey = "AzureBlobStorageConnectionString";

        private static void ConfigureAzureBlobStorageStagingStore(string connectionString, ContainerBuilder builder)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new Exception("Invalid configuration. Missing connection string with name 'AzureBlobStorageConnectionString'.");
            }
            var storageAccount = CloudStorageAccount.Parse(connectionString);
            var cloudBlobClient = storageAccount.CreateCloudBlobClient();
            var stagingStoreContainer = new CachingStagingStoreContainerDecorator(new AzureBlobStorageStagingStoreContainer(cloudBlobClient));
            stagingStoreContainer.PopulateCache().Wait();
            builder.Register(context => stagingStoreContainer).As<IStagingStoreContainer>().SingleInstance();
        }

        private static void StartComponent<T>(IContainer container)
            where T : IComponent
        {
            IComponent component = container.Resolve<T>();
            component.Start();
        }
    }
}
