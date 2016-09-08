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
    class FailComponentWorkCommandHandler : ICommandHandler<FailComponentWorkCommand>
    {
        private IStagingStoreContainer _stagingStoreContainer;
        private IComponentPlanExecutor _componentPlanExecutor;

        public FailComponentWorkCommandHandler(IStagingStoreContainer stagingStoreContainer, IComponentPlanExecutor componentPlanExecutor)
        {
            this._stagingStoreContainer = stagingStoreContainer;
            this._componentPlanExecutor = componentPlanExecutor;
        }

        public Task Handle(FailComponentWorkCommand command)
        {
            throw new NotImplementedException();
        }
    }
}
