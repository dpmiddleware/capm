using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoF.CaPM.IngestSaga.Events
{
    public struct IngestComponentWorkFailed : IIngestEvent
    {
        public string EventType { get => nameof(IngestComponentWorkFailed); }
        public Guid ComponentExecutionId { get; set; }
        public DateTimeOffset Timestamp { get; set; }
        public Guid IngestId { get; set; }
        public string Reason { get; set; }
        public uint Order { get; set; }
    }
}
