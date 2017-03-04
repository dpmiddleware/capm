using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Reactive.Linq;
using System.IO;
using Microsoft.WindowsAzure.Storage.Queue;
using Microsoft.WindowsAzure.Storage;

namespace PoF.Messaging.ServiceBus
{
    public class ServiceBusMessageChannel
    {
        private ChannelIdentifier _channelIdentifier;
        private Subject<string> _subject = new Subject<string>();
        private string _storageAccountConnectionstring;
        private CloudQueue _queue;
        private bool _isListening = false;
        private object _isListeningLockObject = new object();
        private Task _listeningTask;

        private ServiceBusMessageChannel(ChannelIdentifier channelIdentifier, string storageAccountConnectionstring)
        {
            _channelIdentifier = channelIdentifier;
            _storageAccountConnectionstring = storageAccountConnectionstring;
        }

        public static ServiceBusMessageChannel Create(ChannelIdentifier channelIdentifier, string storageAccountConnectionstring)
        {
            var channel = new ServiceBusMessageChannel(channelIdentifier, storageAccountConnectionstring);
            channel.Initialize();
            return channel;
        }

        public void Initialize()
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(_storageAccountConnectionstring);
            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
            _queue = queueClient.GetQueueReference(_channelIdentifier.Name.ToLower().Replace('.', '-'));
            _queue.CreateIfNotExists();
        }

        private void EnsureIsListening()
        {
            if (!_isListening)
            {
                lock (_isListeningLockObject)
                {
                    if (!_isListening)
                    {
                        _isListening = true;
                        _listeningTask = Task.Run(async () =>
                        {
                            while (true)
                            {
                                var message = await _queue.GetMessageAsync(visibilityTimeout: TimeSpan.FromMinutes(5), options: null, operationContext: null);
                                if (message != null)
                                {
                                    var messageContent = message.AsString;
                                    _subject.OnNext(messageContent);
                                    await _queue.DeleteMessageAsync(message);
                                }
                                await Task.Delay(500);
                            }
                        });
                    }
                }
            }
        }

        public IObservable<string> GetChannelObservable()
        {
            return Observable.Create<string>(observer =>
            {
                var subscription = _subject.Subscribe(observer);
                EnsureIsListening();
                return subscription;
            });
        }

        public async Task Send<T>(T message, MessageSendOptions options)
        {
            var serializedMessage = ServiceBusMessageXmlSerializer.Instance.Serialize(message);
            var queueMessage = new CloudQueueMessage(serializedMessage);
            await _queue.AddMessageAsync(queueMessage,
                timeToLive: options.MessageTimeToLiveInSeconds.HasValue ? (TimeSpan?)TimeSpan.FromSeconds(options.MessageTimeToLiveInSeconds.Value) : null,
                initialVisibilityDelay: options.MessageSendDelayInSeconds.HasValue ? (TimeSpan?)TimeSpan.FromSeconds(options.MessageSendDelayInSeconds.Value) : null,
                options: new QueueRequestOptions(),
                operationContext: new OperationContext()
            );
        }
    }
}
