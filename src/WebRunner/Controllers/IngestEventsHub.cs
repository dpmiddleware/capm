using Microsoft.AspNetCore.SignalR;
using PoF.CaPM;
using PoF.CaPM.IngestSaga.Events;
using PoF.CaPM.Serialization;
using PoF.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace WebRunner.Controllers
{
    public class IngestEventsHub : Hub
    {
        internal static void Start(IChannelProvider channelProvider, IComponentChannelIdentifierRepository componentChannelIdentifierRepository, IHubContext<IngestEventsHub> hubContext)
        {
            channelProvider.GetMessageSource<SerializedEvent>(componentChannelIdentifierRepository.GetChannelIdentifierFor(IngestEventConstants.ChannelIdentifierCode)).GetChannel().Subscribe(evt =>
            {
                hubContext.Clients.All.SendAsync("onNewEvent", evt.GetEventObject());
            });
        }
    }
}