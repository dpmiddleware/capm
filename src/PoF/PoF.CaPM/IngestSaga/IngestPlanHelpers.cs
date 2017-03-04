using PoF.CaPM.IngestSaga.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoF.CaPM.IngestSaga
{
    internal static class IngestPlanHelpers
    {
        internal static IngestPlanSet.IngestPlanEntry? GetFirstNonCompletedComponentInCurrentPlan(this IIngestEvent[] storedEvents)
        {
            var currentPlan = storedEvents.OrderBy(e => e.Order).OfType<IngestPlanSet>().Last();
            return GetFirstNonCompletedComponentInPlan(currentPlan, storedEvents);
        }

        private static IngestPlanSet.IngestPlanEntry? GetFirstNonCompletedComponentInPlan(IngestPlanSet lastExecutionPlan, IIngestEvent[] storedEvents)
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

        internal static bool MatchesCurrentlyExecutingComponentExecutionId(this Guid componentExecutionId, IIngestEvent[] storedEvents)
        {
            return storedEvents.GetFirstNonCompletedComponentInCurrentPlan()?.ComponentExecutionId == componentExecutionId;
        }
    }
}
