using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoF.Messaging
{
    public interface IMessageSource<T>
    {
        IObservable<T> GetChannel();
    }
}
