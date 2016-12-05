using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebRunner.Services;
using System.Threading.Tasks;
using WebRunner.Models;

namespace WebRunner.Controllers
{
    public class PreservationSystemHub : Hub
    {
        private IAipStore _store;

        public PreservationSystemHub(IAipStore store)
        {
            _store = store;
        }

        public static void OnNewAip(string aipId)
        {
            var context = GlobalHost.ConnectionManager.GetHubContext<PreservationSystemHub>();
            context.Clients.All.onNewAip(aipId);
        }

        public override async Task OnConnected()
        {
            foreach(var aipId in await _store.GetAllStoredIds())
            {
                Clients.Caller.onNewAip(aipId);
            }
            await base.OnConnected();
        }
    }
}