using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PoF.Messaging;

namespace PoF.CaPM.IngestSaga.Events
{
    public struct IngestPlanSet : IIngestEvent
    {
        public string EventType { get => nameof(IngestPlanSet); }
        public DateTimeOffset Timestamp { get; set; }
        public Guid IngestId { get; set; }
        public IngestPlanEntry[] IngestPlan { get; set; }
        public uint Order { get; set; }

        public struct IngestPlanEntry
        {
            public Guid ComponentExecutionId { get; set; }
            public string ComponentCode { get; set; }
            public int? ExecutionTimeoutInSeconds { get; set; }
            public string ComponentSettings { get; set; }
            public uint Order { get; set; }
            public bool IsCompensatingComponent { get; set; }
        }
    }
}
