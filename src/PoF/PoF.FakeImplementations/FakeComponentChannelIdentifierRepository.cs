using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PoF.Messaging;

namespace PoF.FakeImplementations
{
    public class FakeComponentChannelIdentifierRepository : IComponentChannelIdentifierRepository
    {
        public ChannelIdentifier GetChannelIdentifierFor(string componentCode)
        {
            return new ChannelIdentifier()
            {
                Name = componentCode
            };
        }
    }
}
