using Autofac;
using PoF.Common;
using PoF.Common.Commands.IngestCommands;
using PoF.Messaging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoF.Components.Collector
{
    public class CollectorComponent : IComponent
    {
        public const string CollectorComponentIdentifier = "Component.Collector";
        private ICommandMessageListener _commandMessageListener;

        public CollectorComponent(ICommandMessageListener commandHandlerListener)
        {
            this._commandMessageListener = commandHandlerListener;
        }

        public void Start()
        {
            _commandMessageListener.RegisterCommandHandler<StartComponentWorkCommand, StartCollectorComponentWorkCommandHandler>(CollectorComponentIdentifier);
            _commandMessageListener.RegisterCommandHandler<StartComponentCompensationCommand, StartCollectorComponentCompensationCommandHandler>(CollectorComponentIdentifier);
        }
    }
}
