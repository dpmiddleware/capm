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
using PoF.StagingStore.InMemory;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ComponentRunnerHelpers
{
    public class ComponentRunnerHelper
    {
        public static void AddComponentModule<ComponentAutofacModuleType>(ContainerBuilder builder)
            where ComponentAutofacModuleType : IModule, new()
        {
            builder.RegisterModule<ComponentAutofacModuleType>();
        }

        public static IContainer BootstrapIoCContainer(params Action<ContainerBuilder>[] additionalBootstrappers)
        {
            var builder = new ContainerBuilder();
            IContainer container = null;

            if (additionalBootstrappers != null)
            {
                foreach (var bootstrappingAction in additionalBootstrappers)
                {
                    bootstrappingAction(builder);
                }
            }

            builder.RegisterType<FakeComponentChannelIdentifierRepository>().As<IComponentChannelIdentifierRepository>().SingleInstance();
            builder.RegisterType<CommandMessageListener>().As<ICommandMessageListener>().InstancePerDependency();
            ConfigureMessageSender(builder);
            ConfigureStagingStore(builder);
            builder.Register(context => container).As<IContainer>().SingleInstance();

            container = builder.Build();
            return container;
        }

        private static void ConfigureMessageSender(ContainerBuilder builder)
        {
            //TODO: Should use its own connection string instead of the staging store connection string
            if (string.IsNullOrWhiteSpace(AzureStorageConnectionString))
            {
                builder.Register(_ => InMemoryMessageChannelProvider.Instance).As<InMemoryMessageChannelProvider>().As<IChannelProvider>().SingleInstance();
                builder.RegisterType<InMemoryMessageSenderFactory>().As<IMessageSenderFactory>().SingleInstance();
            }
            else
            {
                builder.Register(_ => new ServiceBusMessageChannelProvider(AzureStorageConnectionString)).As<ServiceBusMessageChannelProvider>().As<IChannelProvider>().SingleInstance();
                builder.RegisterType<ServiceBusMessageSenderFactory>().As<IMessageSenderFactory>().SingleInstance();
            }
        }

        private static void ConfigureStagingStore(ContainerBuilder builder)
        {
            if (string.IsNullOrWhiteSpace(AzureStorageConnectionString))
            {
                ConfigureInMemoryStagingStore(builder);
            }
            else
            {
                ConfigureAzureBlobStorageStagingStore(builder);
            }
        }

        private static void ConfigureInMemoryStagingStore(ContainerBuilder builder)
        {
            builder.RegisterType<InMemoryStagingStoreContainer>().As<IStagingStoreContainer>().SingleInstance();
        }

        private static string AzureStorageConnectionString => ConfigurationManager.ConnectionStrings["AzureBlobStorageStagingStoreConnectionString"]?.ConnectionString;

        private static void ConfigureAzureBlobStorageStagingStore(ContainerBuilder builder)
        {
            var connectionString = AzureStorageConnectionString;
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new Exception("Invalid configuration. Missing connection string with name 'AzureBlobStorageStagingStoreConnectionString'.");
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
