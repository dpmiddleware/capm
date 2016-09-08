using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoF.Common.Commands.IngestCommands
{
    public struct StartIngestCommand
    {
        public string ExternalContextId { get; set; }
        public string SubmissionAgreementId { get; set; }
        public string IngestParameters { get; set; }
    }
}
