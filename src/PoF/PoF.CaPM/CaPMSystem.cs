using Autofac;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using PoF.Messaging;
using PoF.StagingStore;
using System.Reactive;
using System.Reactive.Linq;
using PoF.CaPM.IngestSaga.Events;
using PoF.CaPM.SubmissionAgreements;
using System.Diagnostics;
using PoF.CaPM.Serialization;
using PoF.CaPM.IngestSaga;
using PoF.CaPM.IngestSaga.CaPMCommandHandlers;
using PoF.Common;
using PoF.Common.Commands.IngestCommands;

namespace PoF.CaPM
{
    public class CaPMSystem : IComponent
    {
        public const string CaPMComponentIdentifier = "CaPM";
        private ICommandMessageListener _commandMessageListener;

        public CaPMSystem(ICommandMessageListener commandMessageListener)
        {
            this._commandMessageListener = commandMessageListener;
        }

        public void Start()
        {
            _commandMessageListener.RegisterCommandHandler<StartIngestCommand, StartIngestCommandHandler>(CaPMComponentIdentifier);
            _commandMessageListener.RegisterCommandHandler<FailComponentWorkCommand, FailComponentWorkCommandHandler>(CaPMComponentIdentifier);
            _commandMessageListener.RegisterCommandHandler<CompleteComponentWorkCommand, CompleteComponentWorkCommandHandler>(CaPMComponentIdentifier);
        }
    }
}
