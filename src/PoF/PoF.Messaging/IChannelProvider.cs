using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoF.Messaging
{
    public interface IChannelProvider
    {
        IMessageSource<T> GetMessageSource<T>(ChannelIdentifier identifier);
    }
}
