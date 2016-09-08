using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoF.Common
{
    public interface IComponent
    {
        /// <summary>
        /// Starts the component, e.g. makes it start listening to incoming commands.
        /// </summary>
        /// <remarks>
        /// You should only call Start once on a component. Calling start multiple times can result in unplanned results.
        /// </remarks>
        void Start();
    }
}
