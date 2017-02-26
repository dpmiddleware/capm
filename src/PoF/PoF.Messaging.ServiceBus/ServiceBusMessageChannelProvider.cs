using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoF.Messaging.ServiceBus
{
    public class ServiceBusMessageChannelProvider : IChannelProvider
    {
        private Dictionary<ChannelIdentifier, ServiceBusMessageChannel> _channels;
        private object _channelCreationLockObject;
        private string _storageAccountConnectionstring;

        public ServiceBusMessageChannelProvider(string storageAccountConnectionstring)
        {
            _channels = new Dictionary<ChannelIdentifier, ServiceBusMessageChannel>(new ChannelIdentifierEqualityComparer());
            _channelCreationLockObject = new object();
            _storageAccountConnectionstring = storageAccountConnectionstring;
        }
        
        public ServiceBusMessageChannel GetChannel(ChannelIdentifier identifier)
        {
            if (!_channels.ContainsKey(identifier))
            {
                lock (_channelCreationLockObject)
                {
                    if (!_channels.ContainsKey(identifier))
                    {
                        _channels[identifier] = ServiceBusMessageChannel.Create(identifier, _storageAccountConnectionstring);
                    }
                }
            }
            return _channels[identifier];
        }

        public IMessageSource<T> GetMessageSource<T>(ChannelIdentifier identifier)
        {
            return new ServiceBusMessageSource<T>(GetChannel(identifier));
        }
    }
}
