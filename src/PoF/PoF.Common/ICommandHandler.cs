using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoF.Common
{
    public interface ICommandHandler<T>
    {
        Task Handle(T command);
    }
}
