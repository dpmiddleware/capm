using PoF.StagingStore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PoF.CaPM.IngestSaga.Events;
using PoF.CaPM.IngestSaga;

namespace PoF.CaPM
{
    public class CaPMEventStore : ICaPMEventStore
    {
        private IStagingStoreContainer _stagingStoreContainer;

        public CaPMEventStore(IStagingStoreContainer stagingStoreContainer)
        {
            _stagingStoreContainer = stagingStoreContainer;
        }

        public async Task<IIngestEvent[]> GetAllIngestEvents()
        {
            List<IIngestEvent> results = new List<IngestSaga.Events.IIngestEvent>();
            foreach(var ingestId in await _stagingStoreContainer.GetStoredContextIds())
            {
                var ingestEventStore = await CaPMIngestEventStore.GetCaPMEventStore(_stagingStoreContainer, ingestId);
                results.AddRange(await ingestEventStore.GetStoredEvents().ConfigureAwait(false));
            }
            return results.ToArray();
        }
    }
}
