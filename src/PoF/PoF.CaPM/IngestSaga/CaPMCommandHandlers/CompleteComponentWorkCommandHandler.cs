using PoF.CaPM.IngestSaga.Events;
using PoF.CaPM.Serialization;
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
    class CompleteComponentWorkCommandHandler : ICommandHandler<CompleteComponentWorkCommand>
    {
        private IStagingStoreContainer _stagingStoreContainer;
        private IComponentPlanExecutor _componentPlanExecutor;
        private IMessageSenderFactory _messageSenderFactory;
        private IComponentChannelIdentifierRepository _componentChannelIdentifierRepository;

        public CompleteComponentWorkCommandHandler(IMessageSenderFactory messageSenderFactory, IComponentChannelIdentifierRepository componentChannelIdentifierRepository, IStagingStoreContainer stagingStoreContainer, IComponentPlanExecutor componentPlanExecutor)
        {
            this._stagingStoreContainer = stagingStoreContainer;
            this._componentPlanExecutor = componentPlanExecutor;
            this._messageSenderFactory = messageSenderFactory;
            this._componentChannelIdentifierRepository = componentChannelIdentifierRepository;
        }

        public async Task Handle(CompleteComponentWorkCommand command)
        {
            var eventStore = await CaPMIngestEventStore.GetCaPMEventStore(_stagingStoreContainer, command.IngestId);
            var allPreviousEvents = await eventStore.GetStoredEvents();
            //In case the component has already been considered timeout out (or, for some erraneous reason
            //has already reported that it has errored or completed) we want to ignore this message.
            var currentlyExecutingComponent = allPreviousEvents.GetFirstNonCompletedComponentInCurrentPlan();
            if (currentlyExecutingComponent.HasValue && currentlyExecutingComponent.Value.ComponentExecutionId == command.ComponentExecutionId)
            {
                if (currentlyExecutingComponent.Value.IsCompensatingComponent)
                {
                    await eventStore.StoreEvent(new IngestComponentCompensationCompleted
                    {
                        ComponentExecutionId = command.ComponentExecutionId
                    }, _messageSenderFactory.GetChannel<SerializedEvent>(_componentChannelIdentifierRepository.GetChannelIdentifierFor(IngestEventConstants.ChannelIdentifierCode)));
                }
                else
                {
                    await eventStore.StoreEvent(new IngestComponentWorkCompleted()
                    {
                        ComponentExecutionId = command.ComponentExecutionId
                    }, _messageSenderFactory.GetChannel<SerializedEvent>(_componentChannelIdentifierRepository.GetChannelIdentifierFor(IngestEventConstants.ChannelIdentifierCode)));
                }
                await _componentPlanExecutor.ExecuteNextComponentInPlan(command.IngestId);
            }
        }
    }
}
