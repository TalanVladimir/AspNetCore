using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace AspNetCore
{
    [Authorize]
    public class NotificationHub : Hub
    {
        public async Task SendMessage(string user, string message)
        {
            //await Clients.All.SendAsync("ReceiveMessage", Context.User.Identity.Name, message);
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }
    }
}
