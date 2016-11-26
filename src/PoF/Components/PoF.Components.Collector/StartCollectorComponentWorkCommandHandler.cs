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

        public async Task Handle(StartComponentWorkCommand command)
        {
            try
            {
                var client = new HttpClient();
                var store = await _stagingStoreContainer.GetSharedStore(command.IngestId);
                var response = await client.GetAsync(command.IngestParameters).ConfigureAwait(false);
                var fileStream = await response.Content.ReadAsStreamAsync();
                await store.SetItemAsync("downloadedfile-bytes", fileStream);
                await store.SetItemAsync("downloadedfile-contenttype", response.Content.Headers.ContentType.ToString());
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
