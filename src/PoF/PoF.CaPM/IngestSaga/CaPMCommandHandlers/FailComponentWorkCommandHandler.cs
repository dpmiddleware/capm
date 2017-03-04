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
    class FailComponentWorkCommandHandler : ICommandHandler<FailComponentWorkCommand>
    {
        private IStagingStoreContainer _stagingStoreContainer;
        private IComponentPlanExecutor _componentPlanExecutor;
        private IMessageSenderFactory _messageSenderFactory;
        private IComponentChannelIdentifierRepository _componentChannelIdentifierRepository;

        public FailComponentWorkCommandHandler(IMessageSenderFactory messageSenderFactory, IComponentChannelIdentifierRepository componentChannelIdentifierRepository, IStagingStoreContainer stagingStoreContainer, IComponentPlanExecutor componentPlanExecutor)
        {
            this._stagingStoreContainer = stagingStoreContainer;
            this._componentPlanExecutor = componentPlanExecutor;
            this._messageSenderFactory = messageSenderFactory;
            this._componentChannelIdentifierRepository = componentChannelIdentifierRepository;
        }

        public async Task Handle(FailComponentWorkCommand command)
        {
            var eventStore = await CaPMIngestEventStore.GetCaPMEventStore(_stagingStoreContainer, command.IngestId);
            var allPreviousEvents = await eventStore.GetStoredEvents();
            //In case the component has already been considered timeout out (or, for some erraneous reason
            //has already reported that it has errored or completed) we want to ignore this message.
            if (command.ComponentExecutionId.MatchesCurrentlyExecutingComponentExecutionId(allPreviousEvents))
            {
                var previousPlan = allPreviousEvents.OfType<IngestPlanSet>().Last();
                await eventStore.StoreEvent(new IngestComponentWorkFailed()
                {
                    ComponentExecutionId = command.ComponentExecutionId,
                    Reason = command.Reason
                }, _messageSenderFactory.GetChannel<SerializedEvent>(_componentChannelIdentifierRepository.GetChannelIdentifierFor(IngestEventConstants.ChannelIdentifierCode)));
                var ingestPlan = new IngestPlanSet()
                {
                    IngestPlan = previousPlan.IngestPlan
                        .Reverse()
                        //Only compensate actions which have finished successfully
                        .Where(p => allPreviousEvents.OfType<IngestComponentWorkCompleted>().Select(e => e.ComponentExecutionId).Contains(p.ComponentExecutionId))
                        .Select((eventToCompensate, index) => new IngestPlanSet.IngestPlanEntry()
                        {
                            ComponentCode = eventToCompensate.ComponentCode,
                            ComponentSettings = eventToCompensate.ComponentSettings,
                            ComponentExecutionId = Guid.NewGuid(),
                            IsCompensatingComponent = true,
                            Order = (uint)index
                        }).ToArray()
                };
                await eventStore.StoreEvent(ingestPlan, _messageSenderFactory.GetChannel<SerializedEvent>(_componentChannelIdentifierRepository.GetChannelIdentifierFor(IngestEventConstants.ChannelIdentifierCode)));
                await _componentPlanExecutor.ExecuteNextComponentInPlan(command.IngestId);
            }
        }
    }
}
