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
            private static readonly MessageSendOptions _defaultMessageSendOptions = new MessageSendOptions()
            {
                MessageSendDelayInSeconds = null,
                MessageTimeToLiveInSeconds = null
            };

            public InMemoryMessageSender(InMemoryMessageChannel channel)
            {
                _channel = channel;
            }

            public Task Send(T message)
            {
                return Send(message, _defaultMessageSendOptions);
            }

            public void Dispose()
            {
                _channel = null;
                _isDisposed = true;
            }

            public Task Send(T message, MessageSendOptions options)
            {
                if (_isDisposed)
                {
                    throw new ObjectDisposedException("InMemoryMessageSender");
                }
                if (options.MessageSendDelayInSeconds.HasValue)
                {
                    Task.Run(async () =>
                    {
                        await Task.Delay(TimeSpan.FromSeconds(options.MessageSendDelayInSeconds.Value));
                        _channel.Send(message);
                    });
                }
                else
                {
                    _channel.Send(message);
                }
                return Task.CompletedTask;
            }
        }
    }
}
