using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoF.CaPM.IngestSaga.Events
{
    public interface IIngestEvent
    {
        DateTimeOffset Timestamp { get; set; }
        Guid IngestId { get; set; }
        uint Order { get; set; }
    }
}
