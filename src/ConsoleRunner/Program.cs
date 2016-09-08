using Autofac;
using PoF.CaPM;
using PoF.CaPM.SubmissionAgreements;
using PoF.Common;
using PoF.Common.Commands.IngestCommands;
using PoF.Components.Archiver;
using PoF.Components.Collector;
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
            IContainer container = BootstrapIoCContainer();
            StartComponent<CollectorComponent>(container);
            StartComponent<ArchiverComponent>(container);
            StartComponent<CaPMSystem>(container);
            Task.Factory.StartNew(() =>
            {
                Thread.Sleep(1000);
                var messageSenderFactory = container.Resolve<IMessageSenderFactory>();
                var channelIdentifierRepository = container.Resolve<IComponentChannelIdentifierRepository>();
                var capmMessageChannelIdentifier = channelIdentifierRepository.GetChannelIdentifierFor(CaPMSystem.CaPMComponentIdentifier);
                var capmMessageChannel = messageSenderFactory.GetChannel<StartIngestCommand>(capmMessageChannelIdentifier);
                capmMessageChannel.Send(new StartIngestCommand()
                {
                    SubmissionAgreementId = "SubmissionAgreement1",
                    IngestParameters = "http://images4.fanpop.com/image/photos/16000000/Cute-Kitten-Wallpaper-kittens-16094684-1280-800.jpg"
                });
            });

            Console.WriteLine("System running, press [ENTER] to quit.");
            Console.ReadLine();
            Console.WriteLine("System shut down.");
        }

        private static void StartComponent<T>(IContainer container)
            where T: IComponent
        {
            IComponent component = container.Resolve<T>();
            component.Start();
        }

        private static IContainer BootstrapIoCContainer()
        {
            var builder = new ContainerBuilder();
            IContainer container = null;

            builder.RegisterModule<CaPMAutofacModule>();
            builder.RegisterModule<CollectorAutofacModule>();
            builder.RegisterModule<ArchiverAutofacModule>();

            builder.RegisterType<FakeComponentChannelIdentifierRepository>().As<IComponentChannelIdentifierRepository>().SingleInstance();
            builder.RegisterType<FakeSubmissionAgreementStore>().As<ISubmissionAgreementStore>().SingleInstance();
            builder.RegisterType<CaPMSystem>().SingleInstance();
            builder.RegisterType<CollectorComponent>().SingleInstance();
            builder.RegisterType<ArchiverComponent>().SingleInstance();
            builder.RegisterType<CommandMessageListener>().As<ICommandMessageListener>().InstancePerDependency();
            builder.RegisterType<InMemoryMessageSenderFactory>().As<IMessageSenderFactory>().SingleInstance();
            builder.RegisterType<InMemoryMessageSource>().As<IMessageSource>().SingleInstance();
            builder.RegisterType<InMemoryStagingStoreContainer>().As<IStagingStoreContainer>().SingleInstance();
            builder.Register(context => container).As<IContainer>().SingleInstance();

            container = builder.Build();
            return container;
        }
    }
}
