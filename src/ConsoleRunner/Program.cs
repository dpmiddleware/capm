using Autofac;
using ComponentRunnerHelpers;
using Microsoft.Extensions.Configuration;
using PoF.CaPM;
using PoF.CaPM.SubmissionAgreements;
using PoF.Common;
using PoF.Common.Commands.IngestCommands;
using PoF.Components.Archiver;
using PoF.Components.Collector;
using PoF.Components.RandomError;
using PoF.FakeImplementations;
using PoF.Messaging;
using PoF.Messaging.InMemory;
using PoF.StagingStore;
using PoF.StagingStore.InMemory;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleRunner
{
    class Program
    {
        static void Main(string[] args)
        {
            var configuration = ComponentRunnerHelper.GetConfiguration(args);
            Start(configuration);

            Console.WriteLine("System running, press [ENTER] to quit.");
            Console.ReadLine();
            Console.WriteLine("System shut down.");
        }

        private enum StartMode
        {
            CapmOnly,
            InMemoryWithAllComponents,
            OnlySendLotsOfMessages
        }

        private static void Start(IConfiguration configuration)
        {
            var mode = Enum.Parse(typeof(StartMode), configuration?["mode"] ?? "CapmOnly", ignoreCase: true);
            Console.WriteLine("Starting console runner with mode: " + mode);
            IContainer container;
            switch (mode)
            {
                case StartMode.CapmOnly:
                    container = BootstrapIoCContainer(configuration);
                    StartComponent<CaPMSystem>(container);
                    break;
                case StartMode.InMemoryWithAllComponents:
                    container = BoostrapIoCContainerForInMemoryExecution();
                    StartComponent<CollectorComponent>(container);
                    StartComponent<RandomErrorComponent>(container);
                    StartComponent<ArchiverComponent>(container);
                    StartComponent<CaPMSystem>(container);
                    Task.Factory.StartNew(async () =>
                    {
                        await Task.Delay(2000);
                        SendIngestCommand(container);
                    });
                    break;
                case StartMode.OnlySendLotsOfMessages:
                    container = BootstrapIoCContainer(configuration);
                    SendStartIngestCommands(container);
                    break;
                default:
                    throw new NotSupportedException();
            }
        }

        private static void SendStartIngestCommands(IContainer container)
        {
            Task.Factory.StartNew(() =>
            {
                var messageSenderFactory = container.Resolve<IMessageSenderFactory>();
                var channelIdentifierRepository = container.Resolve<IComponentChannelIdentifierRepository>();
                var capmMessageChannelIdentifier = channelIdentifierRepository.GetChannelIdentifierFor(CaPMSystem.CaPMComponentIdentifier);
                var capmMessageChannel = messageSenderFactory.GetChannel<StartIngestCommand>(capmMessageChannelIdentifier);

                var command = new StartIngestCommand()
                {
                    SubmissionAgreementId = "1% Failing and 1% Failing on Compensation",
                    IngestParameters = "http://localhost:17729/images/unnamed.png"
                };
                for (var i = 0; i < 1302; i++)
                {
                    if (i % 100 == 0)
                    {
                        Console.WriteLine("Sending new message, " + i);
                    }
                    capmMessageChannel.Send(command);
                }
            });
        }

        private static void SendIngestCommand(IContainer container)
        {
            var messageSenderFactory = container.Resolve<IMessageSenderFactory>();
            var channelIdentifierRepository = container.Resolve<IComponentChannelIdentifierRepository>();
            var capmMessageChannelIdentifier = channelIdentifierRepository.GetChannelIdentifierFor(CaPMSystem.CaPMComponentIdentifier);
            var capmMessageChannel = messageSenderFactory.GetChannel<StartIngestCommand>(capmMessageChannelIdentifier);

            var command = new StartIngestCommand()
            {
                SubmissionAgreementId = "SubmissionAgreement1",
                IngestParameters = "http://localhost:17729/images/unnamed.png"
            };

            capmMessageChannel.Send(command);
        }

        private static void StartComponent<T>(IContainer container)
            where T : IComponent
        {
            IComponent component = container.Resolve<T>();
            component.Start();
        }

        private static IContainer BootstrapIoCContainer(IConfiguration configuration)
        {
            return ComponentRunnerHelper.BootstrapIoCContainer(configuration, builder =>
            {
                builder.RegisterType<FakeSubmissionAgreementStore>().As<ISubmissionAgreementStore>().SingleInstance();
                builder.RegisterType<CaPMSystem>();
                builder.RegisterModule<CaPMAutofacModule>();
            });
        }

        private static IContainer BoostrapIoCContainerForInMemoryExecution()
        {
            var builder = new ContainerBuilder();

            builder.RegisterModule<CaPMAutofacModule>();
            builder.RegisterModule<CollectorAutofacModule>();
            builder.RegisterModule<ArchiverAutofacModule>();
            builder.RegisterModule<RandomErrorAutofacModule>();

            builder.RegisterType<FakeComponentChannelIdentifierRepository>().As<IComponentChannelIdentifierRepository>().SingleInstance();
            builder.RegisterType<FakeSubmissionAgreementStore>().As<ISubmissionAgreementStore>().SingleInstance();
            builder.RegisterType<CaPMSystem>().SingleInstance();
            builder.RegisterType<CollectorComponent>().SingleInstance();
            builder.RegisterType<ArchiverComponent>().SingleInstance();
            builder.RegisterType<CommandMessageListener>().As<ICommandMessageListener>().InstancePerDependency();
            builder.Register(_ => InMemoryMessageChannelProvider.Instance).As<IChannelProvider>().SingleInstance();
            builder.RegisterType<InMemoryMessageSenderFactory>().As<IMessageSenderFactory>().SingleInstance();
            builder.RegisterType<InMemoryStagingStoreContainer>().As<IStagingStoreContainer>().SingleInstance();

            var container = builder.Build();
            return container;
        }
    }
}
