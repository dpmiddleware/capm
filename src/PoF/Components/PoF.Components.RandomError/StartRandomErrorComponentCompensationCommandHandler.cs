using PoF.Common;
using PoF.Common.Commands.IngestCommands;
using PoF.Messaging;
using System.Threading.Tasks;

namespace PoF.Components.RandomError
{
    public class StartRandomErrorComponentCompensationCommandHandler : ICommandHandler<StartComponentCompensationCommand>
    {
        private IMessageSenderFactory _messageSenderFactory;

        public StartRandomErrorComponentCompensationCommandHandler(IMessageSenderFactory messageSenderFactory)
        {
            this._messageSenderFactory = messageSenderFactory;
        }

        public async Task Handle(StartComponentCompensationCommand command)
        {
            var settings = RandomErrorComponentSettings.GetSettings(command.ComponentSettings);
            if (!settings.SkipDelay)
            {
                await Task.Delay((int)(RandomErrorComponent._randomizer.NextDouble() * 2 * 1000));
            }
            if (RandomErrorComponent._randomizer.NextDouble() < settings.CompensationFailureRisk)
            {
                await _messageSenderFactory.GetChannel<FailComponentWorkCommand>(command.ComponentResultCallbackChannel).Send(new FailComponentWorkCommand()
                {
                    ComponentExecutionId = command.ComponentExecutionId,
                    IngestId = command.IngestId,
                    Reason = "The randomizer has chosen to give this compensation execution an error"
                });
            }
            else
            {
                await _messageSenderFactory.GetChannel<CompleteComponentWorkCommand>(command.ComponentResultCallbackChannel).Send(new CompleteComponentWorkCommand()
                {
                    ComponentExecutionId = command.ComponentExecutionId,
                    IngestId = command.IngestId
                });
            }
        }
    }
}