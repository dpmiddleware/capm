using Autofac;
using PoF.Components.Archiver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArchiverComponentRunner
{
    class Program
    {
        static void Main(string[] args)
        {
            var container = ComponentRunnerHelpers.ComponentRunnerHelper.BootstrapIoCContainer(builder =>
            {
                builder.RegisterModule<ArchiverAutofacModule>();
            });
            container.Resolve<ArchiverComponent>().Start();
            Console.WriteLine("Archiver component started. Press [ENTER] to quit.");
            Console.ReadLine();
        }
    }
}
