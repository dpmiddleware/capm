using PoF.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoF.CaPM.SubmissionAgreements
{
    public struct SubmissionAgreement
    {
        public string SubmissionAgreementId { get; set; }
        public ComponentExecutionPlan[] ProcessComponents { get; set; }

        public struct ComponentExecutionPlan
        {
            public string ComponentCode { get; set; }
            public int? ExecutionTimeoutInSeconds { get; set; }
            public string ComponentSettings { get; set; }
            public uint Order { get; set; }
        }
    }
}
