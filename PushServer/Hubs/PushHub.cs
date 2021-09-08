using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PushServer.Hubs
{
    public class PushHub : Hub
    {
        public async Task SendPush(string appId, string message)
        {
            await Clients.All.SendAsync("ReceivePush", appId, message);
        }
    }
}
