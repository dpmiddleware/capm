using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoF.CaPM.IngestSaga.Events
{
    public struct IngestStarted : IIngestEvent
    {
        public DateTimeOffset Timestamp { get; set; }
        public Guid IngestId  { get; set; }
        public string ExternalContextId { get; set; }
        public string IngestParameters { get; set; }
        public uint Order { get; set; }
    }
}
