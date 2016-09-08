using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoF.Messaging.InMemory
{
    public class InMemoryMessageSenderFactory : IMessageSenderFactory
    {
        public IMessageSender<T> GetChannel<T>(ChannelIdentifier identifier)
        {
            var channel = InMemoryMessageChannelProvider.Instance.GetChannel(identifier);
            return new InMemoryMessageSender<T>(channel);
        }

        private class InMemoryMessageSender<T> : IMessageSender<T>, IDisposable
        {
            private InMemoryMessageChannel _channel;
            private bool _isDisposed = false;

            public InMemoryMessageSender(InMemoryMessageChannel channel)
            {
                _channel = channel;
            }

            public Task Send(T message)
            {
                if (_isDisposed)
                {
                    throw new ObjectDisposedException("InMemoryMessageSender");
                }
                _channel.Send(message);
                return Task.CompletedTask;
            }

            public void Dispose()
            {
                _channel = null;
                _isDisposed = true;
            }
        }
    }
}
