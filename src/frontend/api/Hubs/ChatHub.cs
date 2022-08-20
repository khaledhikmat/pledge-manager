using Microsoft.AspNetCore.SignalR;

namespace pledgemanager.frontend.api.Hubs;

public class ChatHub : Hub
{
    public async Task SendMessage(string message)
    {
        await Clients.All.SendAsync("MessageReceived", message);
    }
}
