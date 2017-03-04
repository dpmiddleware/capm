using System;
using System.Threading.Tasks;

namespace PoF.Messaging.ServiceBus
{
    internal class ServiceBusMessageSender<T> : IMessageSender<T>
    {
        private ServiceBusMessageChannel _channel;
        private static readonly MessageSendOptions _defaultMessageSendOptions = new MessageSendOptions()
        {
            MessageSendDelayInSeconds = null
        };

        public ServiceBusMessageSender(ServiceBusMessageChannel channel)
        {
            this._channel = channel;
        }

        public void Dispose()
        {
        }

        public async Task Send(T message)
        {
            await _channel.Send(message, _defaultMessageSendOptions);
        }

        public async Task Send(T message, MessageSendOptions options)
        {
            await _channel.Send(message, options);
        }
    }
}