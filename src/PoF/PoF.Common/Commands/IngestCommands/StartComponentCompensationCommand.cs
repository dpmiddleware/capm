using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PoF.Messaging;

namespace PoF.Common.Commands.IngestCommands
{
    public struct StartComponentCompensationCommand
    {
        public Guid IngestId { get; set; }
        public string ComponentCode { get; set; }
        public Guid ComponentExecutionId { get; set; }
        public ChannelIdentifier ComponentResultCallbackChannel { get; set; }
        public string ComponentSettings { get; set; }
        public string IngestParameters { get; set; }
    }
}
