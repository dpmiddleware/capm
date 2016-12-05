using PoF.CaPM.IngestSaga.Events;
using PoF.CaPM.Serialization;
using PoF.CaPM.SubmissionAgreements;
using PoF.Common;
using PoF.Common.Commands.IngestCommands;
using PoF.Messaging;
using PoF.StagingStore;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoF.CaPM.IngestSaga.CaPMCommandHandlers
{
    internal class StartIngestCommandHandler : ICommandHandler<StartIngestCommand>
    {
        private ISubmissionAgreementStore _submissionAgreementStore;
        private IStagingStoreContainer _stagingStoreContainer;
        private IComponentPlanExecutor _componentPlanExecutor;
        private IMessageSenderFactory _messageSenderFactory;
        private IComponentChannelIdentifierRepository _componentChannelIdentifierRepository;

        public StartIngestCommandHandler(IMessageSenderFactory messageSenderFactory, IComponentChannelIdentifierRepository componentChannelIdentifierRepository, ISubmissionAgreementStore submissionAgreementStore, IStagingStoreContainer stagingStoreContainer, IComponentPlanExecutor componentPlanExecutor)
        {
            this._submissionAgreementStore = submissionAgreementStore;
            this._stagingStoreContainer = stagingStoreContainer;
            this._componentPlanExecutor = componentPlanExecutor;
            this._messageSenderFactory = messageSenderFactory;
            this._componentChannelIdentifierRepository = componentChannelIdentifierRepository;
        }

        public async Task Handle(StartIngestCommand command)
        {
            // We currently assume that the service that have sent the StartIngestCommand has already verified that the caller has authority
            // to invoke the specified submission agreement, so we just use the agreement specified. 
            SubmissionAgreement submissionAgreement;
            try
            {
                submissionAgreement = _submissionAgreementStore.Get(command.SubmissionAgreementId);
            }
            catch (KeyNotFoundException)
            {
                Debug.WriteLine($"There is no submission agreement with the ID '{command.SubmissionAgreementId}'. Aborting!");
                return;
            }

            var newIngestId = Guid.NewGuid();
            var ingestStartedEvent = new IngestStarted()
            {
                IngestParameters = command.IngestParameters,
                ExternalContextId = command.ExternalContextId
            };
            var eventStore = await CaPMIngestEventStore.CreateCaPMEventStore(_stagingStoreContainer, newIngestId).ConfigureAwait(false);

            await eventStore.StoreEvent(ingestStartedEvent, _messageSenderFactory.GetChannel<SerializedEvent>(_componentChannelIdentifierRepository.GetChannelIdentifierFor(IngestEventConstants.ChannelIdentifierCode))).ConfigureAwait(false);
            await CreateIngestPlanBasedOnSubmissionAgreement(eventStore, submissionAgreement).ConfigureAwait(false);
            await _componentPlanExecutor.ExecuteNextComponentInPlan(newIngestId).ConfigureAwait(false);
        }

        private async Task CreateIngestPlanBasedOnSubmissionAgreement(CaPMIngestEventStore eventStore, SubmissionAgreement submissionAgreement)
        {
            var ingestPlan = new IngestPlanSet()
            {
                IngestPlan = submissionAgreement.ProcessComponents.Select((processComponent, index) => new IngestPlanSet.IngestPlanEntry()
                {
                    ComponentCode = processComponent.ComponentCode,
                    ComponentExecutionId = Guid.NewGuid(),
                    ComponentSettings = processComponent.ComponentSettings,
                    IsCompensatingComponent = false,
                    Order = (uint)index
                }).ToArray()
            };
            await eventStore.StoreEvent(ingestPlan, _messageSenderFactory.GetChannel<SerializedEvent>(_componentChannelIdentifierRepository.GetChannelIdentifierFor(IngestEventConstants.ChannelIdentifierCode)));
        }
    }
}
