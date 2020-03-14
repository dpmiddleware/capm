using Autofac;
using Autofac.Extensions.DependencyInjection;
using ComponentRunnerHelpers;
using Newtonsoft.Json;
using PoF.CaPM;
using PoF.CaPM.SubmissionAgreements;
using PoF.Components.Archiver;
using PoF.Components.Collector;
using PoF.Components.RandomError;
using PoF.FakeImplementations;
using System;
using System.Linq;
using System.IO;
using System.Reflection;
using WebRunner.Controllers;
using WebRunner.Services;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace WebRunner
{
    internal static class AutofacConfig
    {
        public static void BootstrapIoCContainer(ContainerBuilder builder, IConfiguration configuration)
        {
            ValidateComponentsToRunConfiguration(configuration);
            //builder.RegisterApiControllers(Assembly.GetExecutingAssembly());
            builder.Register(context => CreateSubmissionAgreementStore()).As<ISubmissionAgreementStore>().SingleInstance();
            ComponentRunnerHelper.AddComponentModule<CaPMAutofacModule>(builder);
            LoadComponents(configuration, builder);
            builder.RegisterType<CaPMSystem>().SingleInstance();
            builder.RegisterType<CaPMEventStore>().As<ICaPMEventStore>().SingleInstance();
            ConfigureAipStore(configuration, builder);
            builder.RegisterType<IngestEventsHub>().InstancePerDependency();
            builder.RegisterType<PreservationSystemHub>().InstancePerDependency();
            ComponentRunnerHelper.BootstrapIoCContainer(builder, configuration);
        }

        private static void LoadComponents(IConfiguration configuration, ContainerBuilder builder)
        {
            foreach (var componentName in GetComponentsToRun(configuration))
            {
                var componentModule = (Autofac.Core.IModule)Activator.CreateInstance(ModuleTypeDictionary[componentName].Assembly.GetType($"PoF.Components.{componentName}.{componentName}AutofacModule"));
                builder.RegisterModule(componentModule);
            }
        }

        public static IReadOnlyDictionary<string, Type> ModuleTypeDictionary = new System.Collections.ObjectModel.ReadOnlyDictionary<string, Type>(new Dictionary<string, Type>()
        {
            ["Archiver"] = typeof(ArchiverComponent),
            ["Collector"] = typeof(CollectorComponent),
            ["RandomError"] = typeof(RandomErrorComponent)
        });

        private static void ValidateComponentsToRunConfiguration(IConfiguration configuration)
        {
            foreach (var componentName in GetComponentsToRun(configuration))
            {
                if (!ModuleTypeDictionary.ContainsKey(componentName))
                {
                    throw new NotSupportedException($"Application configuration invalid. Component name '{componentName}' is not a supported component. Valid component names are: {string.Join(", ", ModuleTypeDictionary.Keys)}");
                }
            }
        }

        public static string[] GetComponentsToRun(IConfiguration configuration) =>
            configuration["ComponentsToRun"]?.Split(',', ';').Select(c => c.Trim()).ToArray() ?? new string[0];

        private static string GetAzureStorageConnectionString(IConfiguration configuration) =>
            configuration["AzureBlobStorageConnectionString"];

        private static void ConfigureAipStore(IConfiguration configuration, ContainerBuilder builder)
        {
            IAipStore store;
            //TODO: Should use its own storage connection string instead of using the Staging Store connection string
            var connectionString = GetAzureStorageConnectionString(configuration);
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