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
        private IMessageSource _messageSource;
        private IComponentChannelIdentifierRepository _componentChannelIdentifierRepository;
        private IContainer _iocContainer;

        public CommandMessageListener(IMessageSource messageSource, IComponentChannelIdentifierRepository componentChannelIdentifierRepository, IContainer iocContainer)
        {
            this._messageSource = messageSource;
            this._componentChannelIdentifierRepository = componentChannelIdentifierRepository;
            this._iocContainer = iocContainer;
        }

        public IDisposable RegisterCommandHandler<Command, CommandHandler>(string messageChannelIdentifierCode)
            where CommandHandler : ICommandHandler<Command>
        {
            ChannelIdentifier messageChannelIdentifier = _componentChannelIdentifierRepository.GetChannelIdentifierFor(messageChannelIdentifierCode);
            return _messageSource.RegisterCommandHandler<Command, CommandHandler>(messageChannelIdentifier, _iocContainer);
        }
    }
}
