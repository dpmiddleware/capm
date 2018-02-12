using Autofac;
using PoF.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoF.CaPM.IngestSaga.CaPMCommandHandlers
{
    class CommandHandlerAutofacModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<StartIngestCommandHandler>().InstancePerDependency();
            builder.RegisterType<CompleteComponentWorkCommandHandler>().InstancePerDependency();
            builder.RegisterType<FailComponentWorkCommandHandler>().InstancePerDependency();
            builder.RegisterType<TimeoutComponentWorkCommandHandler>().InstancePerDependency();
        }
    }
}
