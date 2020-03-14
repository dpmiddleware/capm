using Autofac;
using PoF.Components.Collector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollectorComponentRunner
{
    class Program
    {
        static void Main(string[] args)
        {
            var configuration = ComponentRunnerHelpers.ComponentRunnerHelper.GetConfiguration(args);
            var container = ComponentRunnerHelpers.ComponentRunnerHelper.BootstrapIoCContainer(configuration,
                builder =>
                {
                    builder.RegisterModule<CollectorAutofacModule>();
                }
            );
            container.Resolve<CollectorComponent>().Start();
            Console.WriteLine("Collector component started. Press [ENTER] to quit.");
            Console.ReadLine();
        }
    }
}
