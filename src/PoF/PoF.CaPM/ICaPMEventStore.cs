using PoF.CaPM.IngestSaga.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoF.CaPM
{
    public interface ICaPMEventStore
    {
        //TODO: This method really needs filter/page/other so that it does not return all events for all ingests, but not necessary for this POC
        Task<IIngestEvent[]> GetAllIngestEvents();
    }
}
