﻿using PoF.Common;
using PoF.Common.Commands.IngestCommands;
using PoF.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace PoF.Components.RandomError
{
    class StartRandomErrorComponentWorkCommandHandler : ICommandHandler<StartComponentWorkCommand>
    {
        private IMessageSenderFactory _messageSenderFactory;
        private static readonly Random _randomizer = new Random();

        public StartRandomErrorComponentWorkCommandHandler(IMessageSenderFactory messageSenderFactory)
        {
            this._messageSenderFactory = messageSenderFactory;
        }

        public async Task Handle(StartComponentWorkCommand command)
        {
            if (_randomizer.NextDouble() > 0.5)
            { 
                await _messageSenderFactory.GetChannel<CompleteComponentWorkCommand>(command.ComponentResultCallbackChannel).Send(new CompleteComponentWorkCommand()
                {
                    ComponentExecutionId = command.ComponentExecutionId,
                    IngestId = command.IngestId
                });
            }
            else
            {
                await _messageSenderFactory.GetChannel<FailComponentWorkCommand>(command.ComponentResultCallbackChannel).Send(new FailComponentWorkCommand()
                {
                    ComponentExecutionId = command.ComponentExecutionId,
                    IngestId = command.IngestId,
                    Reason = "The randomizer has chosen to give this execution an error"
                });
            }
        }
    }
}