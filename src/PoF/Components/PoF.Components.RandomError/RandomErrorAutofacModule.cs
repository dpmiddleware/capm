using Autofac;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoF.Components.RandomError
{
    public class RandomErrorAutofacModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<StartRandomErrorComponentWorkCommandHandler>().InstancePerDependency();
            builder.RegisterType<StartRandomErrorComponentCompensationCommandHandler>().InstancePerDependency();
            builder.RegisterType<RandomErrorComponent>().SingleInstance();
        }
    }
}
