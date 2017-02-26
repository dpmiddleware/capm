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
using Newtonsoft.Json;

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
            return channelProvider.GetMessageSource<Command>(channelIdentifier).GetChannel().Subscribe(command =>
            {
                var correlationId = Guid.NewGuid();
                WriteToConsole<Command, CommandHandler>(correlationId, "Starting message processing");
                WriteToConsole<Command, CommandHandler>(correlationId, JsonConvert.SerializeObject(command, Formatting.None));
                iocContainer.Resolve<CommandHandler>().Handle(command);
                WriteToConsole<Command, CommandHandler>(correlationId, "message processing completed");
            });
        }

        private static void WriteToConsole<Command, CommandHandler>(Guid correlationId, string message) 
        {
            Console.WriteLine($"{DateTimeOffset.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")}: [{typeof(CommandHandler).FullName}] - {correlationId} - {message}");
        }
    }
}
