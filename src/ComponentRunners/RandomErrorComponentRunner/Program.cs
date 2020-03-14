using Autofac;
using PoF.Components.RandomError;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RandomErrorComponentRunner
{
    class Program
    {
        static void Main(string[] args)
        {
            var configuration = ComponentRunnerHelpers.ComponentRunnerHelper.GetConfiguration(args);
            var container = ComponentRunnerHelpers.ComponentRunnerHelper.BootstrapIoCContainer(configuration,
                builder =>
                {
                    builder.RegisterModule<RandomErrorAutofacModule>();
                }
            );
            container.Resolve<RandomErrorComponent>().Start();
            Console.WriteLine("RandomError component started. Press [ENTER] to quit.");
            Console.ReadLine();
        }
    }
}
