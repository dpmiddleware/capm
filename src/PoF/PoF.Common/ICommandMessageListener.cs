using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoF.Common
{
    public interface ICommandMessageListener
    {
        IDisposable RegisterCommandHandler<Command, CommandHandler>(string messageChannelIdentifierCode)
            where CommandHandler : ICommandHandler<Command>;
    }
}
