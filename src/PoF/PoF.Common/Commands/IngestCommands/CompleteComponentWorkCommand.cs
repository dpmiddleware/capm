using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoF.Common.Commands.IngestCommands
{
    public struct CompleteComponentWorkCommand
    {
        public Guid ComponentExecutionId { get; set; }
        public Guid IngestId { get; set; }
    }
}
