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

namespace PoF.Components.RandomError
{
    public class RandomErrorComponent : IComponent
    {
        public const string RandomErrorComponentIdentifier = "Component.RandomError";
        private ICommandMessageListener _commandHandlerListener;

        public RandomErrorComponent(ICommandMessageListener commandHandlerListener)
        {
            this._commandHandlerListener = commandHandlerListener;
        }

        public void Start()
        {
            _commandHandlerListener.RegisterCommandHandler<StartComponentWorkCommand, StartRandomErrorComponentWorkCommandHandler>(RandomErrorComponentIdentifier);
            _commandHandlerListener.RegisterCommandHandler<StartComponentCompensationCommand, StartRandomErrorComponentCompensationCommandHandler>(RandomErrorComponentIdentifier);
        }
    }
}
