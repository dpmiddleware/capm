using System;
using System.Threading.Tasks;
using PoF.Common;
using PoF.Common.Commands.IngestCommands;
using PoF.Messaging;

namespace PoF.Components.Archiver
{
    internal class StartArchiverComponentCompensationCommandHandler : ICommandHandler<StartComponentCompensationCommand>
    {
        private IMessageSenderFactory _messageSenderFactory;

        public StartArchiverComponentCompensationCommandHandler(IMessageSenderFactory messageSenderFactory)
        {
            this._messageSenderFactory = messageSenderFactory;
        }

        public async Task Handle(StartComponentCompensationCommand command)
        {
            //There is nothing this component can compensate, which is why it needs to be the last component in a workflow
            await _messageSenderFactory.GetChannel<CompleteComponentWorkCommand>(command.ComponentResultCallbackChannel).Send(new CompleteComponentWorkCommand()
            {
                ComponentExecutionId = command.ComponentExecutionId,
                IngestId = command.IngestId
            });
        }
    }
}