using Newtonsoft.Json;
using PoF.Common;
using PoF.Common.Commands.IngestCommands;
using PoF.Messaging;
using PoF.StagingStore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace PoF.Components.Collector
{
    class StartCollectorComponentCompensationCommandHandler : ICommandHandler<StartComponentCompensationCommand>
    {
        private IStagingStoreContainer _stagingStoreContainer;
        private IMessageSenderFactory _messageSenderFactory;
        private static readonly Random _randomizer = new Random();

        public StartCollectorComponentCompensationCommandHandler(IStagingStoreContainer stagingStoreContainer, IMessageSenderFactory messageSenderFactory)
        {
            this._stagingStoreContainer = stagingStoreContainer;
            this._messageSenderFactory = messageSenderFactory;
        }

        public async Task Handle(StartComponentCompensationCommand command)
        {
            var store = await _stagingStoreContainer.GetSharedStore(command.IngestId);
            var settings = GetSettings(command);
            if (settings.CompensationFailureRisk.HasValue && _randomizer.NextDouble() < settings.CompensationFailureRisk.Value)
            {
                await _messageSenderFactory.GetChannel<FailComponentWorkCommand>(command.ComponentResultCallbackChannel).Send(new FailComponentWorkCommand()
                {
                    ComponentExecutionId = command.ComponentExecutionId,
                    IngestId = command.IngestId,
                    Reason = "Randomized compensation failure occured"
                });
            }
            else
            {
                await store.RemoveItemAsync("downloadedfile-bytes");
                await store.RemoveItemAsync("downloadedfile-contenttype");
                await _messageSenderFactory.GetChannel<CompleteComponentWorkCommand>(command.ComponentResultCallbackChannel).Send(new CompleteComponentWorkCommand()
                {
                    ComponentExecutionId = command.ComponentExecutionId,
                    IngestId = command.IngestId
                });
            }
        }

        private CollectorComponentSettings GetSettings(StartComponentCompensationCommand command)
        {
            try
            {
                return JsonConvert.DeserializeObject<CollectorComponentSettings>(command.ComponentSettings);
            }
            catch
            {
                return new CollectorComponentSettings();
            }
        }
    }
}
