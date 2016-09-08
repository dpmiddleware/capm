using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoF.Common
{
    public struct ComponentContext
    {
        public Guid IngestId { get; set; }
        public string ComponentSettings { get; set; }
        public Guid IngestStepId { get; set; }
        public string IngestParameters { get; set; }
    }
}
