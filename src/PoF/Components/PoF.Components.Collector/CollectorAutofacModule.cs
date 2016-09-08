using Autofac;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoF.Components.Collector
{
    public class CollectorAutofacModule:Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<StartCollectorComponentWorkCommandHandler>().InstancePerDependency();
        }
    }
}
