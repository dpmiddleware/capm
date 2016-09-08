using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoF.Messaging.InMemory
{
    internal class InMemoryMessageChannelProvider
    {
        private Dictionary<ChannelIdentifier, InMemoryMessageChannel> _channels;
        private object _channelCreationLockObject;

        private InMemoryMessageChannelProvider()
        {
            _channels = new Dictionary<ChannelIdentifier, InMemoryMessageChannel>(new ChannelIdentifierEqualityComparer());
            _channelCreationLockObject = new object();
        }
        
        public InMemoryMessageChannel GetChannel(ChannelIdentifier identifier)
        {
            if (!_channels.ContainsKey(identifier))
            {
                lock (_channelCreationLockObject)
                {
                    if (!_channels.ContainsKey(identifier))
                    {
                        _channels[identifier] = new InMemoryMessageChannel();
                    }
                }
            }
            return _channels[identifier];
        }

        public static InMemoryMessageChannelProvider Instance { get; } = new InMemoryMessageChannelProvider();
    }
}
