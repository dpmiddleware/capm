using PoF.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoF.Messaging
{
    public interface IComponentChannelIdentifierRepository
    {
        ChannelIdentifier GetChannelIdentifierFor(string componentCode);
    }
}
