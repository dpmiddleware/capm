using PoF.Common;
using PoF.Common.Commands.IngestCommands;
using System;

namespace PoF.Components.RandomError
{
    public class RandomErrorComponent : IComponent
    {
        public const string RandomErrorComponentIdentifier = "Component.RandomError";
        private ICommandMessageListener _commandHandlerListener;
        internal static readonly Random _randomizer = new Random();

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
