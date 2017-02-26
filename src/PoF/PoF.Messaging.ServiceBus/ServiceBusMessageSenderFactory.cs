using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoF.Messaging.ServiceBus
{
    public class ServiceBusMessageSenderFactory : IMessageSenderFactory
    {
        private ServiceBusMessageChannelProvider _messageChannelProvider;

        public ServiceBusMessageSenderFactory(ServiceBusMessageChannelProvider messageChannelProvider)
        {
            _messageChannelProvider = messageChannelProvider;
        }

        public IMessageSender<T> GetChannel<T>(ChannelIdentifier identifier)
        {
            var channel = _messageChannelProvider.GetChannel(identifier);
            return new ServiceBusMessageSender<T>(channel);
        }
    }
}
