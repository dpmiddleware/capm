using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoF.Messaging
{
    internal class ChannelIdentifierEqualityComparer : IEqualityComparer<ChannelIdentifier>
    {
        public bool Equals(ChannelIdentifier x, ChannelIdentifier y)
        {
            return x.Name.Equals(y.Name);
        }

        public int GetHashCode(ChannelIdentifier obj)
        {
            return obj.Name.GetHashCode();
        }
    }
}
