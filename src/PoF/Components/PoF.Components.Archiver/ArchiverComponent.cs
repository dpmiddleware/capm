using PoF.Common;
using PoF.Common.Commands.IngestCommands;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoF.Components.Archiver
{
    public class ArchiverComponent : IComponent
    {
        public const string ArchiverComponentIdentifier = "Component.Archiver";
        private ICommandMessageListener _commandMessageListener;

        public ArchiverComponent(ICommandMessageListener commandMessageListener)
        {
            this._commandMessageListener = commandMessageListener;
        }

        public void Start()
        {
            _commandMessageListener.RegisterCommandHandler<StartComponentWorkCommand, StartArchiverComponentWorkCommandHandler>(ArchiverComponentIdentifier);
            _commandMessageListener.RegisterCommandHandler<StartComponentCompensationCommand, StartArchiverComponentCompensationCommandHandler>(ArchiverComponentIdentifier);
        }
    }
}
