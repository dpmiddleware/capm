using System;
using System.Threading.Tasks;

namespace PoF.Messaging.ServiceBus
{
    internal class ServiceBusMessageSender<T> : IMessageSender<T>
    {
        private ServiceBusMessageChannel _channel;

        public ServiceBusMessageSender(ServiceBusMessageChannel channel)
        {
            this._channel = channel;
        }

        public void Dispose()
        {
        }

        public async Task Send(T message)
        {
            await _channel.Send<T>(message);
        }
    }
}