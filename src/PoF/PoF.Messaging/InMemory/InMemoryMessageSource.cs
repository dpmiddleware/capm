using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reactive.Linq;

namespace PoF.Messaging.InMemory
{
    public class InMemoryMessageSource : IMessageSource
    {
        public IObservable<T> GetChannel<T>(ChannelIdentifier identifier)
        {
            var type = typeof(T);
            var channel = InMemoryMessageChannelProvider.Instance.GetChannel(identifier);
            return channel.GetChannelObservable()
                .Where(t => t.Contains(type.Name))
                .Select(s =>
                {
                    T output;
                    var result = InMemoryMessageXmlSerializer.Instance.TryDeserialize<T>(s, out output);
                    return new
                    {
                        CouldDeserializeToT = result,
                        Output = output
                    };
                })
                .Where(m => m.CouldDeserializeToT)
                .Select(m => m.Output); ;
        }
    }
}
