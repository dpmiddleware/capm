using PoF.Common.Commands.IngestCommands;
using PoF.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoF.CaPM.IngestSaga.Events
{
    public struct IngestComponentCompensationStartRequested : IIngestEvent
    {
        public DateTimeOffset Timestamp { get; set; }
        public Guid IngestId { get; set; }
        public uint Order { get; set; }
        public StartComponentCompensationCommand CommandSent { get; internal set; }
    }
}
