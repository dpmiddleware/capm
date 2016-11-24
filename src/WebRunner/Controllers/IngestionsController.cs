using PoF.CaPM;
using PoF.CaPM.IngestSaga.Events;
using PoF.Common.Commands.IngestCommands;
using PoF.Messaging;
using PoF.StagingStore;
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
        private ICaPMEventStore _capmEventStore;
        private IComponentChannelIdentifierRepository _componentChannelIdentifierRepository;
        private IMessageSenderFactory _messageSenderFactory;

        public IngestionsController(IMessageSenderFactory messageSenderFactory, IComponentChannelIdentifierRepository componentChannelIdentifierRepository, ICaPMEventStore capmEventStore)
        {
            _messageSenderFactory = messageSenderFactory;
            _componentChannelIdentifierRepository = componentChannelIdentifierRepository;
            _capmEventStore = capmEventStore;
        }

        [HttpGet]
        public async Task<KeyValuePair<Guid, IIngestEvent[]>[]> Get()
        {
            return await _capmEventStore.GetAllIngestEvents();
        }

        [HttpGet]
        public async Task<IHttpActionResult> Get(Guid id)
        {
            var allIngestEvents = await _capmEventStore.GetAllIngestEvents();
            if (allIngestEvents.Any(i => i.Key == id))
            {
                return Ok(allIngestEvents.First(i => i.Key == id).Value);
            }
            else
            {
                return NotFound();
            }
        }

        [HttpPost]
        public async Task Post(StartIngestionModel model)
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