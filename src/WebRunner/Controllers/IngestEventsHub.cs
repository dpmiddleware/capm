using Microsoft.AspNet.SignalR;
using PoF.CaPM;
using PoF.CaPM.IngestSaga.Events;
using PoF.CaPM.Serialization;
using PoF.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Dependencies;

namespace WebRunner.Controllers
{
    public class IngestEventsHub : Hub
    {
        static IngestEventsHub()
        {
            var channelProvider = (IChannelProvider)GlobalConfiguration.Configuration.DependencyResolver.GetService(typeof(IChannelProvider));
            var componentChannelIdentifierRepository = (IComponentChannelIdentifierRepository)GlobalConfiguration.Configuration.DependencyResolver.GetService(typeof(IComponentChannelIdentifierRepository));
            var hubContext = GlobalHost.ConnectionManager.GetHubContext<IngestEventsHub>();
            channelProvider.GetMessageSource<SerializedEvent>(componentChannelIdentifierRepository.GetChannelIdentifierFor(IngestEventConstants.ChannelIdentifierCode)).GetChannel().Subscribe(evt =>
            {
                hubContext.Clients.All.onNewEvent(evt.GetEventObject());
            });
        }
    }
}