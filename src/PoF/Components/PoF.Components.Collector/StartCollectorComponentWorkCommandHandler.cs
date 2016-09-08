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
    class StartCollectorComponentWorkCommandHandler : ICommandHandler<StartComponentWorkCommand>
    {
        private IStagingStoreContainer _stagingStoreContainer;
        private IMessageSenderFactory _messageSenderFactory;

        public StartCollectorComponentWorkCommandHandler(IStagingStoreContainer stagingStoreContainer, IMessageSenderFactory messageSenderFactory)
        {
            this._stagingStoreContainer = stagingStoreContainer;
            this._messageSenderFactory = messageSenderFactory;
        }

        private async Task<IComponentStagingStore> GetStore(Guid ingestId)
        {
            var ingestStore = await _stagingStoreContainer.GetStoreForContextIdAsync(ingestId);
            return await ingestStore.GetComponentStagingStoreAsync(CollectorComponent.CollectorComponentIdentifier);
        }

        public async Task Handle(StartComponentWorkCommand command)
        {
            try
            {
                var client = new HttpClient();
                var store = await GetStore(command.IngestId);
                var fileStream = await client.GetStreamAsync(command.IngestParameters);
                await store.SetItemAsync("downloadedfile", fileStream);
                await _messageSenderFactory.GetChannel<CompleteComponentWorkCommand>(command.ComponentResultCallbackChannel).Send(new CompleteComponentWorkCommand()
                {
                    ComponentExecutionId = command.ComponentExecutionId,
                    IngestId = command.IngestId
                });
            }
            catch(Exception e)
            {
                await _messageSenderFactory.GetChannel<FailComponentWorkCommand>(command.ComponentResultCallbackChannel).Send(new FailComponentWorkCommand()
                {
                    ComponentExecutionId = command.ComponentExecutionId,
                    IngestId = command.IngestId,
                    Reason = e.ToString()
                });
            }
        }
    }
}
