using PoF.Common.Commands.IngestCommands;
using PoF.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoF.CaPM.IngestSaga.Events
{
    public struct IngestComponentWorkStartRequested : IIngestEvent
    {
        public string EventType { get => nameof(IngestComponentWorkStartRequested); }
        public DateTimeOffset Timestamp { get; set; }
        public Guid IngestId { get; set; }
        public uint Order { get; set; }
        public StartComponentWorkCommand CommandSent { get; set; }
    }
}
