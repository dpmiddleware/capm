using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoF.CaPM.IngestSaga.Events
{
    public interface IIngestEvent
    {
        string EventType { get; }
        DateTimeOffset Timestamp { get; set; }
        Guid IngestId { get; set; }
        uint Order { get; set; }
    }
}
