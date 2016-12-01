using PoF.CaPM.IngestSaga;
using PoF.CaPM.IngestSaga.Events;
using PoF.CaPM.Serialization;
using PoF.Common.Commands.IngestCommands;
using PoF.Messaging;
using PoF.StagingStore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoF.CaPM
{
    internal class ComponentPlanExecutor : IComponentPlanExecutor
    {
        private IComponentChannelIdentifierRepository _componentChannelIdentifierRepository;
        private IMessageSenderFactory _messageSenderFactory;
        private IStagingStoreContainer _stagingStoreContainer;

        public ComponentPlanExecutor(IComponentChannelIdentifierRepository componentChannelIdentifierRepository, IMessageSenderFactory messageSenderFactory, IStagingStoreContainer stagingStoreContainer)
        {
            this._componentChannelIdentifierRepository = componentChannelIdentifierRepository;
            this._messageSenderFactory = messageSenderFactory;
            this._stagingStoreContainer = stagingStoreContainer;
        }

        public async Task ExecuteNextComponentInPlan(Guid ingestId)
        {
            var eventStore = await GetCaPMEventStore(ingestId);
            var storedEvents = await eventStore.GetStoredEvents();
            var lastExecutionPlan = storedEvents.OfType<IngestPlanSet>().Last();
            var firstNonCompletedComponentInPlan = GetFirstNonCompletedComponentInPlan(lastExecutionPlan, storedEvents);
            if (firstNonCompletedComponentInPlan.HasValue)
            {
                if (firstNonCompletedComponentInPlan.Value.IsCompensatingComponent)
                {
                    await ExecuteComponentCompensation(ingestId, firstNonCompletedComponentInPlan.Value);
                }
                else
                {
                    await ExecuteComponentWork(ingestId, firstNonCompletedComponentInPlan.Value);
                }
            }
            else
            {
                await eventStore.StoreEvent(new IngestCompleted(), _messageSenderFactory.GetChannel<SerializedEvent>(_componentChannelIdentifierRepository.GetChannelIdentifierFor(IngestEventConstants.ChannelIdentifierCode)));
            }
        }

        private async Task ExecuteComponentWork(Guid ingestId, IngestPlanSet.IngestPlanEntry planEntry)
        {
            var eventStore = await GetCaPMEventStore(ingestId);
            var ingestStartedEvent = (await eventStore.GetStoredEvents()).OfType<IngestStarted>().Single();
            var plannedComponentChannelIdentifier = _componentChannelIdentifierRepository.GetChannelIdentifierFor(planEntry.ComponentCode);
            var messageChannel = _messageSenderFactory.GetChannel<StartComponentWorkCommand>(plannedComponentChannelIdentifier);
            var command = new StartComponentWorkCommand()
            {
                IngestId = ingestId,
                ComponentCode = planEntry.ComponentCode,
                ComponentExecutionId = planEntry.ComponentExecutionId,
                ComponentSettings = planEntry.ComponentSettings,
                ComponentResultCallbackChannel = GetCaPMMessageChannelIdentifier(),
                IngestParameters = ingestStartedEvent.IngestParameters
            };
            //We would need to do the following two as part of a transaction, if we want to make any guarantees
            await eventStore.StoreEvent(new IngestComponentWorkStartRequested()
            {
                CommandSent = command
            }, _messageSenderFactory.GetChannel<SerializedEvent>(_componentChannelIdentifierRepository.GetChannelIdentifierFor(IngestEventConstants.ChannelIdentifierCode)));
            await messageChannel.Send(command);
        }

        private async Task ExecuteComponentCompensation(Guid ingestId, IngestPlanSet.IngestPlanEntry planEntry)
        {
            var eventStore = await GetCaPMEventStore(ingestId);
            var ingestStartedEvent = (await eventStore.GetStoredEvents()).OfType<IngestStarted>().Single();
            var plannedComponentChannelIdentifier = _componentChannelIdentifierRepository.GetChannelIdentifierFor(planEntry.ComponentCode);
            var messageChannel = _messageSenderFactory.GetChannel<StartComponentCompensationCommand>(plannedComponentChannelIdentifier);
            var command = new StartComponentCompensationCommand()
            {
                IngestId = ingestId,
                ComponentCode = planEntry.ComponentCode,
                ComponentExecutionId = planEntry.ComponentExecutionId,
                ComponentSettings = planEntry.ComponentSettings,
                ComponentResultCallbackChannel = GetCaPMMessageChannelIdentifier(),
                IngestParameters = ingestStartedEvent.IngestParameters
            };
            //We would need to do the following two as part of a transaction, if we want to make any guarantees
            await eventStore.StoreEvent(new IngestComponentCompensationStartRequested()
            {
                CommandSent = command
            }, _messageSenderFactory.GetChannel<SerializedEvent>(_componentChannelIdentifierRepository.GetChannelIdentifierFor(IngestEventConstants.ChannelIdentifierCode)));
            await messageChannel.Send(command);
        }

        private IngestPlanSet.IngestPlanEntry? GetFirstNonCompletedComponentInPlan(IngestPlanSet lastExecutionPlan, IIngestEvent[] storedEvents)
        {
            var completedComponentExecutionIds =
                storedEvents.OfType<IngestComponentWorkCompleted>().Select(e => e.ComponentExecutionId).Concat(
                    storedEvents.OfType<IngestComponentCompensationCompleted>().Select(e => e.ComponentExecutionId)
                ).ToArray();
            var orderedPlanEntries = lastExecutionPlan.IngestPlan.OrderBy(p => p.Order);
            foreach (var planEntry in orderedPlanEntries)
            {
                if (!completedComponentExecutionIds.Contains(planEntry.ComponentExecutionId))
                {
                    return planEntry;
                }
            }
            //If all the entries in the plan has already been completed, we return a nulled nullable to indicate that no more entries are in the plan
            return null;
        }

        private Task<CaPMIngestEventStore> GetCaPMEventStore(Guid ingestId)
        {
            return CaPMIngestEventStore.GetCaPMEventStore(_stagingStoreContainer, ingestId);
        }

        private ChannelIdentifier GetCaPMMessageChannelIdentifier()
        {
            return _componentChannelIdentifierRepository.GetChannelIdentifierFor(CaPMSystem.CaPMComponentIdentifier);
        }
    }
}
