using Autofac;
using Autofac.Core;
using PoF.CaPM.IngestSaga.CaPMCommandHandlers;
using PoF.CaPM.SubmissionAgreements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoF.CaPM
{
    public class CaPMAutofacModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<ComponentPlanExecutor>().As<IComponentPlanExecutor>().SingleInstance();

            builder.RegisterModule<CommandHandlerAutofacModule>();
        }
    }
}
