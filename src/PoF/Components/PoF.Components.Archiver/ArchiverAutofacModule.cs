using Autofac;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoF.Components.Archiver
{
    public class ArchiverAutofacModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<StartArchiverComponentWorkCommandHandler>().InstancePerDependency();
            builder.RegisterType<StartArchiverComponentCompensationCommandHandler>().InstancePerDependency();
            builder.RegisterType<ArchiverComponent>().SingleInstance();
        }
    }
}
