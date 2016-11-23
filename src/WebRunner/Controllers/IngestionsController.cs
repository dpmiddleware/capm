using PoF.CaPM;
using PoF.Common.Commands.IngestCommands;
using PoF.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using WebRunner.Models;

namespace WebRunner.Controllers
{
    public class IngestionsController : ApiController
    {
        private IComponentChannelIdentifierRepository _componentChannelIdentifierRepository;
        private IMessageSenderFactory _messageSenderFactory;

        public IngestionsController(IMessageSenderFactory messageSenderFactory, IComponentChannelIdentifierRepository componentChannelIdentifierRepository)
        {
            _messageSenderFactory = messageSenderFactory;
            _componentChannelIdentifierRepository = componentChannelIdentifierRepository;
        }

        [HttpPost]
        public async Task Post(IngestionModel model)
        {
            if (ModelState.IsValid)
            {
                var capmMessageChannelIdentifier = _componentChannelIdentifierRepository.GetChannelIdentifierFor(CaPMSystem.CaPMComponentIdentifier);
                var capmMessageChannel = _messageSenderFactory.GetChannel<StartIngestCommand>(capmMessageChannelIdentifier);
                await capmMessageChannel.Send(new StartIngestCommand()
                {
                    SubmissionAgreementId = model.SubmissionAgreementId,
                    IngestParameters = model.IngestParameters
                });
            }
            else
            {
                throw new ArgumentException("Invalid model posted");
            }
        }
    }
}