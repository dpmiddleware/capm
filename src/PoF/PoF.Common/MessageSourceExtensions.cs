using Autofac;
using PoF.Common;
using PoF.Messaging;
using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoF.Common
{
    public static class MessageSourceExtensions
    {
        public static IDisposable RegisterCommandHandler<Command, CommandHandler>(this IChannelProvider channelProvider, ChannelIdentifier channelIdentifier, IContainer iocContainer)
            where CommandHandler : ICommandHandler<Command>
        {
            // Resolve the commmand handler once to verify that it is correctly registered in the iocContainer.
            // This is done to force the method to fail early (when making the registration) instead of late (when receiving a message of the given command type).
            iocContainer.Resolve<CommandHandler>();
            return channelProvider.GetMessageSource<Command>(channelIdentifier).GetChannel().Subscribe(command => iocContainer.Resolve<CommandHandler>().Handle(command));
        }
    }
}
