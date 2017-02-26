using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reactive.Linq;

namespace PoF.Messaging.ServiceBus
{
    public class ServiceBusMessageSource<T> : IMessageSource<T>
    {
        private ServiceBusMessageChannel _channel;

        public ServiceBusMessageSource(ServiceBusMessageChannel channel)
        {
            _channel = channel;
        }

        public IObservable<T> GetChannel()
        {
            var type = typeof(T);
            return _channel.GetChannelObservable()
                .Where(t => t.Contains(type.Name))
                .Select(s =>
                {
                    T output;
                    var result = ServiceBusMessageXmlSerializer.Instance.TryDeserialize<T>(s, out output);
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
