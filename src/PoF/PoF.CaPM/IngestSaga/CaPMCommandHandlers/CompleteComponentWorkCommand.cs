using PoF.CaPM.IngestSaga.Events;
using PoF.Common;
using PoF.Common.Commands.IngestCommands;
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

        public CompleteComponentWorkCommandHandler(IStagingStoreContainer stagingStoreContainer, IComponentPlanExecutor componentPlanExecutor)
        {
            this._stagingStoreContainer = stagingStoreContainer;
            this._componentPlanExecutor = componentPlanExecutor;
        }

        public async Task Handle(CompleteComponentWorkCommand command)
        {
            var eventStore = await CaPMIngestEventStore.GetCaPMEventStore(_stagingStoreContainer, command.IngestId);
            await eventStore.StoreEvent(new IngestComponentWorkCompleted()
            {
                ComponentExecutionId = command.ComponentExecutionId
            });
            await _componentPlanExecutor.ExecuteNextComponentInPlan(command.IngestId);
        }
    }
}
