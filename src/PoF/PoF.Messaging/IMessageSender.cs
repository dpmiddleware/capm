using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoF.Messaging
{
    public interface IMessageSender<T> : IDisposable
    {
        Task Send(T message);
        Task Send(T message, MessageSendOptions options);
    }
}
