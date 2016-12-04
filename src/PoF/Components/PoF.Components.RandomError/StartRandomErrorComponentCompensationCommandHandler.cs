using PoF.Common;
using PoF.Common.Commands.IngestCommands;
using PoF.Messaging;
using System.Threading.Tasks;

namespace PoF.Components.RandomError
{
    public class StartRandomErrorComponentCompensationCommandHandler: ICommandHandler<StartComponentCompensationCommand>
    {
        private IMessageSenderFactory _messageSenderFactory;

        public StartRandomErrorComponentCompensationCommandHandler(IMessageSenderFactory messageSenderFactory)
        {
            this._messageSenderFactory = messageSenderFactory;
        }

        public async Task Handle(StartComponentCompensationCommand command)
        {
            await _messageSenderFactory.GetChannel<CompleteComponentWorkCommand>(command.ComponentResultCallbackChannel).Send(new CompleteComponentWorkCommand()
            {
                ComponentExecutionId = command.ComponentExecutionId,
                IngestId = command.IngestId
            });
        }
    }
}