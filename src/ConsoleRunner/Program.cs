using Autofac;
using ComponentRunnerHelpers;
using Microsoft.Extensions.Configuration;
using PoF.CaPM;
using PoF.CaPM.IngestSaga.Events;
using PoF.CaPM.Serialization;
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
using PoF.StagingStore.Filesystem;
using PoF.StagingStore.InMemory;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
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
        }

        private enum StartMode
        {
            CapmOnly,
            InMemoryWithAllComponents,
            OnlySendLotsOfMessages,
            Simulation
        }

        private static void Start(IConfiguration configuration)
        {
            StartMode mode;
            try
            {
                mode = (StartMode)Enum.Parse(typeof(StartMode), configuration?["mode"] ?? "CapmOnly", ignoreCase: true);
            }
            catch (ArgumentException e)
            {
                Console.WriteLine("Failed parsing execution mode. " + e.Message + " Valid values for --mode are: " + string.Join(", ", Enum.GetNames(typeof(StartMode))));
                return;
            }
            Console.WriteLine("Starting console runner with mode: " + mode);
            IContainer container;
            switch (mode)
            {
                case StartMode.CapmOnly:
                    container = BootstrapIoCContainer(configuration);
                    StartComponent<CaPMSystem>(container);
                    break;
                case StartMode.InMemoryWithAllComponents:
                    container = BoostrapIoCContainerForInMemoryExecution().Build();
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
                    var command = new StartIngestCommand()
                    {
                        SubmissionAgreementId = "1% Failing and 1% Failing on Compensation",
                        IngestParameters = "http://localhost:17729/images/unnamed.png"
                    };
                    Task.Factory.StartNew(() => SendStartIngestCommands(container, 1302, _ => command, "Send messages"));
                    break;
                case StartMode.Simulation:
                    var decimalFormattingCulture = CultureInfo.GetCultureInfo("en-US");
                    var isFailureRateProvided = decimal.TryParse(configuration["failure-rate"], NumberStyles.Any, decimalFormattingCulture, out var failureRate);
                    var isCompensationFailureRateProvided = decimal.TryParse(configuration["compensation-failure-rate"], NumberStyles.Any, decimalFormattingCulture, out var compensationFailureRate);
                    var isIngestCountProvided = int.TryParse(configuration["ingest-count"], out var ingestCount);
                    var isComponentCountProvided = int.TryParse(configuration["number-of-components-per-ingest"], out var componentCount);
                    var isStoragePathProvided = !string.IsNullOrEmpty(configuration["storage-path"]);
                    if (isFailureRateProvided && isCompensationFailureRateProvided && isIngestCountProvided && isStoragePathProvided && isComponentCountProvided && componentCount > 0 && ingestCount > 0)
                    {
                        var filePath = configuration["storage-path"];
                        if (File.Exists(filePath))
                        {
                            throw new IOException($"Aborting execution since it will not be possible to write results, file '{filePath}' already exists");
                        }
                        var containerBuilder = BoostrapIoCContainerForInMemoryExecution();
                        MessageSourceExtensions.DisableMessageProcessingConsoleOutput = true;
                        InMemoryComponentStagingStore.SkipDelay = true;
                        //containerBuilder.RegisterInstance(new FileSystemStagingStoreContainer(configuration["storage-path"])).As<IStagingStoreContainer>().SingleInstance();
                        container = containerBuilder.Build();
                        var submissionAgreementStore = container.Resolve<FakeSubmissionAgreementStore>();
                        var simulationAgreementId = "simulationagreement";
                        submissionAgreementStore.Add(simulationAgreementId, new SubmissionAgreement()
                        {
                            SubmissionAgreementId = simulationAgreementId,
                            ProcessComponents = Enumerable.Range(0, componentCount).Select(_ =>
                                new SubmissionAgreement.ComponentExecutionPlan
                                {
                                    ComponentCode = "Component.RandomError",
                                    ComponentSettings = $"{{\"FailureRisk\": {failureRate.ToString(decimalFormattingCulture)}, \"CompensationFailureRisk\": {compensationFailureRate.ToString(decimalFormattingCulture)}, \"SkipDelay\": true }}",
                                    Order = 1
                                }
                            ).ToArray()
                        });
                        StartComponent<CollectorComponent>(container);
                        StartComponent<RandomErrorComponent>(container);
                        StartComponent<ArchiverComponent>(container);
                        StartComponent<CaPMSystem>(container);
                        var ingestCommand = new StartIngestCommand()
                        {
                            SubmissionAgreementId = simulationAgreementId,
                            IngestParameters = ""
                        };
                        var channelProvider = container.Resolve<IChannelProvider>();
                        var componentChannelIdentifierRepository = container.Resolve<IComponentChannelIdentifierRepository>();
                        var eventsByIngestId = new Dictionary<Guid, List<IIngestEvent>>();
                        Func<List<IIngestEvent>, bool> hasFailedCompensation = (events) => events.Any(e => e is IngestComponentCompensationFailed);
                        Func<List<IIngestEvent>, bool> hasFailedIngest = (events) => events.Any(e => e is IngestComponentWorkFailed);
                        var numberOfSuccessfulIngests = 0;
                        var numberOfCompensatedIngests = 0;
                        var numberOfFailedCompensations = 0;
                        var numberOfComponentExecutionsWhichNeedCompensation = 0;
                        var numberOfSuccessfulComponentExecutions = 0;
                        var numberOfFailedComponentExecutions = 0;
                        var numberOfSuccessfulComponentCompensations = 0;
                        var numberOfFailedComponentCompensations = 0;
                        var numberOfComponentExecutionsWhichWouldHaveNeededManualWorkWithoutCompensationResilience = 0;
                        channelProvider.GetMessageSource<SerializedEvent>(componentChannelIdentifierRepository.GetChannelIdentifierFor(IngestEventConstants.ChannelIdentifierCode)).GetChannel().Subscribe(evt =>
                        {
                            var evtObject = evt.GetEventObject();
                            var events = eventsByIngestId.ContainsKey(evtObject.IngestId) ? eventsByIngestId[evtObject.IngestId] : eventsByIngestId[evtObject.IngestId] = new List<IIngestEvent>();
                            events.Add(evtObject);
                            switch (evtObject)
                            {
                                case IngestCompleted ingComp:
                                    if (hasFailedIngest(events))
                                        if (hasFailedCompensation(events))
                                        {
                                            numberOfFailedCompensations++;
                                            var countOfEventsWhichNeedCompensation = events.Count(e => e is IngestComponentWorkCompleted || e is IngestComponentWorkFailed);
                                            var countOfEventsWhichWereCompensatedBeforeFirstCompensationFailure =
                                                events
                                                    .Where(e => e is IngestComponentCompensationCompleted || e is IngestComponentCompensationFailed)
                                                    .TakeWhile(e => !(e is IngestComponentCompensationFailed))
                                                    .Count();
                                            numberOfComponentExecutionsWhichWouldHaveNeededManualWorkWithoutCompensationResilience +=
                                                countOfEventsWhichNeedCompensation - countOfEventsWhichWereCompensatedBeforeFirstCompensationFailure;
                                        }
                                        else
                                            numberOfCompensatedIngests++;
                                    else
                                        numberOfSuccessfulIngests++;
                                    eventsByIngestId.Remove(evtObject.IngestId);
                                    break;
                                case IngestComponentWorkCompleted ingComp:
                                    numberOfSuccessfulComponentExecutions++;
                                    break;
                                case IngestComponentWorkFailed ingComp:
                                    numberOfFailedComponentExecutions++;
                                    numberOfComponentExecutionsWhichNeedCompensation +=
                                        events.Count(e => e is IngestComponentWorkCompleted);
                                    break;
                                case IngestComponentCompensationCompleted ingComp:
                                    numberOfSuccessfulComponentCompensations++;
                                    break;
                                case IngestComponentCompensationFailed ingComp:
                                    numberOfFailedComponentCompensations++;
                                    break;
                                case IngestPlanSet _:
                                case IngestStarted _:
                                case IngestComponentWorkStartRequested _:
                                case IngestComponentCompensationStartRequested _:
                                    //We ignore these events, because we don't want to count them
                                    break;
                                default:
                                    throw new Exception("Unknown event type " + evtObject.GetType().FullName);
                            }
                        });
                        SendStartIngestCommands(container, ingestCount, _ => ingestCommand, $"F: {failureRate}, CF: {compensationFailureRate}");
                        if (File.Exists(filePath))
                        {
                            throw new IOException($"Could not save results, file '{filePath}' already exists");
                        }
                        File.WriteAllText(configuration["storage-path"], JsonSerializer.Serialize(new
                        {
                            numberOfIngests = ingestCount,
                            numberOfSuccessfulIngests,
                            numberOfFailedIngests = ingestCount - numberOfSuccessfulIngests,
                            numberOfCompensatedIngests,
                            numberOfFailedCompensations,
                            failureRate,
                            compensationFailureRate,
                            numberOfSuccessfulComponentExecutions,
                            numberOfSuccessfulComponentCompensations,
                            numberOfFailedComponentExecutions,
                            numberOfFailedComponentCompensations,
                            numberOfComponentExecutionsWhichNeedCompensation,
                            numberOfComponentExecutionsWhichWouldHaveNeededManualWorkWithoutCompensationResilience
                        }, new JsonSerializerOptions
                        {
                            WriteIndented = true,
                            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                        }));
                        return;
                    }
                    else
                    {
                        Console.WriteLine("Mandatory parameters missing. Need to configure the failure rate, compensation failure rate, ingest count and storage path, or pass them as parameters to the cli, e.g. --mode simulation --failure-rate 0.01 --compensation-failure-rate 0.01 --ingest-count 100000 --number-of-components-per-ingest 5 --storage-path C:\\temp\\simulation-results.txt");
                        Console.WriteLine("The following values were missing, invalid or not possible to parse:");
                        if (!isFailureRateProvided)
                        {
                            Console.WriteLine("    --failure-rate:                    " + configuration["failure-rate"]);
                        }
                        if (!isCompensationFailureRateProvided)
                        {
                            Console.WriteLine("    --compensation-failure-rate:       " + configuration["compensation-failure-rate"]);
                        }
                        if (!isIngestCountProvided)
                        {
                            Console.WriteLine("    --ingest-count:                    " + configuration["ingest-count"]);
                        }
                        if (!isComponentCountProvided)
                        {
                            Console.WriteLine("    --number-of-components-per-ingest  " + configuration["number-of-components-per-ingest"]);
                        }
                        if (!isStoragePathProvided)
                        {
                            Console.WriteLine("    --storage-path:                    " + configuration["storage-path"]);
                        }
                        return;
                    }
                default:
                    throw new NotSupportedException();
            }

            Console.WriteLine("System running, press [ENTER] to quit.");
            Console.ReadLine();
            Console.WriteLine("System shut down.");
        }

        private static void SendStartIngestCommands(IContainer container, int numberOfIngestCommandsToSend, Func<int, StartIngestCommand> createCommandForIngestNumberFactory, string testName)
        {
            var messageSenderFactory = container.Resolve<IMessageSenderFactory>();
            var channelIdentifierRepository = container.Resolve<IComponentChannelIdentifierRepository>();
            var capmMessageChannelIdentifier = channelIdentifierRepository.GetChannelIdentifierFor(CaPMSystem.CaPMComponentIdentifier);
            var capmMessageChannel = messageSenderFactory.GetChannel<StartIngestCommand>(capmMessageChannelIdentifier);

            for (var i = 0; i < numberOfIngestCommandsToSend; i++)
            {
                if (i % 100 == 0)
                {
                    Console.WriteLine($"{testName} progress: {i} / {numberOfIngestCommandsToSend}");
                }
                capmMessageChannel.Send(createCommandForIngestNumberFactory(i));
            }
            Console.WriteLine($"{testName} progress: {numberOfIngestCommandsToSend} / {numberOfIngestCommandsToSend}");
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

        private static ContainerBuilder BoostrapIoCContainerForInMemoryExecution()
        {
            var builder = new ContainerBuilder();

            builder.RegisterModule<CaPMAutofacModule>();
            builder.RegisterModule<CollectorAutofacModule>();
            builder.RegisterModule<ArchiverAutofacModule>();
            builder.RegisterModule<RandomErrorAutofacModule>();

            builder.RegisterType<FakeComponentChannelIdentifierRepository>().As<IComponentChannelIdentifierRepository>().SingleInstance();
            builder.RegisterType<FakeSubmissionAgreementStore>().AsSelf().As<ISubmissionAgreementStore>().SingleInstance();
            builder.RegisterType<CaPMSystem>().SingleInstance();
            builder.RegisterType<CollectorComponent>().SingleInstance();
            builder.RegisterType<ArchiverComponent>().SingleInstance();
            builder.RegisterType<CommandMessageListener>().As<ICommandMessageListener>().InstancePerDependency();
            builder.Register(_ => InMemoryMessageChannelProvider.Instance).As<IChannelProvider>().SingleInstance();
            builder.RegisterType<InMemoryMessageSenderFactory>().As<IMessageSenderFactory>().SingleInstance();
            builder.RegisterType<InMemoryStagingStoreContainer>().As<IStagingStoreContainer>().SingleInstance();

            return builder;
        }
    }
}
