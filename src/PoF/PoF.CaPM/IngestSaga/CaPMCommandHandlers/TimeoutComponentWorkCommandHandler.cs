using PoF.Common;
using PoF.Common.Commands.IngestCommands;
using PoF.Messaging;
using PoF.StagingStore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoF.CaPM.IngestSaga.CaPMCommandHandlers
{
    public class TimeoutComponentWorkCommandHandler : ICommandHandler<TimeoutComponentWorkCommand>
    {
        private IComponentChannelIdentifierRepository _componentChannelIdentifierRepository;
        private IMessageSenderFactory _messageSenderFactory;
        private IStagingStoreContainer _stagingStoreContainer;
        
        public TimeoutComponentWorkCommandHandler(IComponentChannelIdentifierRepository componentChannelIdentifierRepository, IMessageSenderFactory messageSenderFactory, IStagingStoreContainer stagingStoreContainer)
        {
            this._componentChannelIdentifierRepository = componentChannelIdentifierRepository;
            this._messageSenderFactory = messageSenderFactory;
            this._stagingStoreContainer = stagingStoreContainer;
        }

        public async Task Handle(TimeoutComponentWorkCommand command)
        {
            var capmEventStore = await CaPMIngestEventStore.GetCaPMEventStore(_stagingStoreContainer, command.IngestId);
            var storedEvents = await capmEventStore.GetStoredEvents();
            //If the currently executing component is the one that is supposed to time out, we'll send 
            //a message that the component failed due to timeout. Otherwise, the component this timeout
            //message is about a component which has already completed its work, in which case we just 
            //ignore this message
            if (command.ComponentExecutionId.MatchesCurrentlyExecutingComponentExecutionId(storedEvents))
            {
                await _messageSenderFactory.GetChannel<FailComponentWorkCommand>(command.ComponentTimeoutMessageChannel).Send(new FailComponentWorkCommand()
                {
                    ComponentExecutionId = command.ComponentExecutionId,
                    IngestId = command.IngestId,
                    Reason = "The component work processing timed out."
                });
            }
        }
    }
}
