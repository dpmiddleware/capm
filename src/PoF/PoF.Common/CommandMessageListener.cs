using Autofac;
using PoF.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoF.Common
{
    public class CommandMessageListener : ICommandMessageListener
    {
        private IChannelProvider _channelProvider;
        private IComponentChannelIdentifierRepository _componentChannelIdentifierRepository;
        private IContainer _iocContainer;

        public CommandMessageListener(IChannelProvider channelProvider, IComponentChannelIdentifierRepository componentChannelIdentifierRepository, IContainer iocContainer)
        {
            this._channelProvider = channelProvider;
            this._componentChannelIdentifierRepository = componentChannelIdentifierRepository;
            this._iocContainer = iocContainer;
        }

        public IDisposable RegisterCommandHandler<Command, CommandHandler>(string messageChannelIdentifierCode)
            where CommandHandler : ICommandHandler<Command>
        {
            ChannelIdentifier messageChannelIdentifier = _componentChannelIdentifierRepository.GetChannelIdentifierFor(messageChannelIdentifierCode);
            return _channelProvider.RegisterCommandHandler<Command, CommandHandler>(messageChannelIdentifier, _iocContainer);
        }
    }
}
