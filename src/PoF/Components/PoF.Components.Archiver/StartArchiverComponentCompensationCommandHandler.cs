using System;
using System.Threading.Tasks;
using PoF.Common;
using PoF.Common.Commands.IngestCommands;

namespace PoF.Components.Archiver
{
    internal class StartArchiverComponentCompensationCommandHandler : ICommandHandler<StartComponentCompensationCommand>
    {
        public Task Handle(StartComponentCompensationCommand command)
        {
            throw new NotSupportedException("The Archive Component currently does not support compensation.");
        }
    }
}