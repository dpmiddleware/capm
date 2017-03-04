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

        public StartCollectorComponentCompensationCommandHandler(IStagingStoreContainer stagingStoreContainer, IMessageSenderFactory messageSenderFactory)
        {
            this._stagingStoreContainer = stagingStoreContainer;
            this._messageSenderFactory = messageSenderFactory;
        }

        public async Task Handle(StartComponentCompensationCommand command)
        {
            var store = await _stagingStoreContainer.GetSharedStore(command.IngestId);
            if (await store.HasItemAsync("downloadedfile-bytes"))
            {
                await store.RemoveItemAsync("downloadedfile-bytes");
            }
            if (await store.HasItemAsync("downloadedfile-contenttype"))
            {
                await store.RemoveItemAsync("downloadedfile-contenttype");
            }
            await _messageSenderFactory.GetChannel<CompleteComponentWorkCommand>(command.ComponentResultCallbackChannel).Send(new CompleteComponentWorkCommand()
            {
                ComponentExecutionId = command.ComponentExecutionId,
                IngestId = command.IngestId
            });
        }
    }
}
