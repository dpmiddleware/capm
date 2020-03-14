using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebRunner.Services;
using System.Threading.Tasks;
using WebRunner.Models;
using Microsoft.AspNetCore.SignalR;

namespace WebRunner.Controllers
{
    public class PreservationSystemHub : Hub
    {
        private static IHubContext<PreservationSystemHub> _hubContext;
        private IAipStore _store;

        public PreservationSystemHub(IAipStore store)
        {
            _store = store;
        }

        internal static void Initialize(IHubContext<PreservationSystemHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public static async Task OnNewAip(string aipId)
        {
            await _hubContext.Clients.All.SendAsync("onNewAip", aipId).ConfigureAwait(false);
        }

        public override async Task OnConnectedAsync()
        {
            foreach (var aipId in await _store.GetAllStoredIds())
            {
                await Clients.Caller.SendAsync("onNewAip", aipId);
            }
            await base.OnConnectedAsync();
        }
    }
}