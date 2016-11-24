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

        public async Task<KeyValuePair<Guid, IIngestEvent[]>[]> GetAllIngestEvents()
        {
            List<KeyValuePair<Guid, IIngestEvent[]>> results = new List<KeyValuePair<Guid, IIngestEvent[]>>();
            foreach(var ingestId in await _stagingStoreContainer.GetStoredContextIds())
            {
                var ingestEventStore = await CaPMIngestEventStore.GetCaPMEventStore(_stagingStoreContainer, ingestId);
                results.Add(new KeyValuePair<Guid, IngestSaga.Events.IIngestEvent[]>(ingestId, await ingestEventStore.GetStoredEvents()));
            }
            return results.ToArray();
        }
    }
}
