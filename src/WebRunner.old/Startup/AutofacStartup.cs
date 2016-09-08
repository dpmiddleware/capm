using Autofac;
using Owin;
using PoF.CaPM;
using PoF.CaPM.SubmissionAgreements;
using PoF.Common;
using PoF.Components.Archiver;
using PoF.Components.Collector;
using PoF.FakeImplementations;
using PoF.Messaging;
using PoF.Messaging.InMemory;
using PoF.StagingStore;
using PoF.StagingStore.InMemory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebRunner.Startup
{
    internal static class AutofacStartup
    {
        public static void Configuration(IAppBuilder app)
        {
            IContainer container = BootstrapIoCContainer();
            StartComponent<CollectorComponent>(container);
            StartComponent<ArchiverComponent>(container);
            StartComponent<CaPMSystem>(container);

            var webApiResolver = new Autofac.Integration.WebApi.AutofacWebApiDependencyResolver(container);
            System.Web.Http.GlobalConfiguration.Configuration.DependencyResolver = webApiResolver;
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