using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Reactive.Linq;
using System.IO;

namespace PoF.Messaging.InMemory
{
    internal class InMemoryMessageChannel
    {
        private ChannelIdentifier _channelIdentifier;
        private Subject<string> _subject = new Subject<string>();

        public InMemoryMessageChannel(ChannelIdentifier channelIdentifier)
        {
            _channelIdentifier = channelIdentifier;
        }

        public IObservable<string> GetChannelObservable()
        {
            return _subject;
        }

        public void Send<T>(T message)
        {
            var serializedMessage = InMemoryMessageXmlSerializer.Instance.Serialize(message);
            _subject.OnNext(serializedMessage);
        }
    }
}
