using Microsoft.AspNetCore.Mvc;
using PoF.CaPM;
using PoF.CaPM.IngestSaga.Events;
using PoF.Common.Commands.IngestCommands;
using PoF.Messaging;
using PoF.StagingStore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebRunner.Models;

namespace WebRunner.Controllers
{
    [Route("api/ingestions")]
    public class IngestionsController : ControllerBase
    {
        private IComponentChannelIdentifierRepository _componentChannelIdentifierRepository;
        private IMessageSenderFactory _messageSenderFactory;
        private ICaPMEventStore _capmEventStore;

        public IngestionsController(ICaPMEventStore capmEventStore, IMessageSenderFactory messageSenderFactory, IComponentChannelIdentifierRepository componentChannelIdentifierRepository)
        {
            _messageSenderFactory = messageSenderFactory;
            _componentChannelIdentifierRepository = componentChannelIdentifierRepository;
            _capmEventStore = capmEventStore;
        }

        [HttpGet]
        public async Task<IIngestEvent[]> Get()
        {
            var events = await _capmEventStore.GetAllIngestEvents().ConfigureAwait(false);
            var sortedEvents = events.OrderBy(e => e.Timestamp).ToArray();
            return sortedEvents;
        }

        [HttpPost]
        public async Task Post([FromBody]StartIngestionModel model)
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
                throw new ArgumentException("Invalid model posted. " + System.Text.Json.JsonSerializer.Serialize(ModelState));
            }
        }
    }
}