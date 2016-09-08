using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoF.CaPM
{
    internal interface IComponentPlanExecutor
    {
        Task ExecuteNextComponentInPlan(Guid ingestId);
    }
}
