using Microsoft.AspNetCore.SignalR;

namespace pledgemanager.frontend.api.Hubs;

public class CampaignHub : Hub
{
    public async Task SendCampaign(Campaign campaign)
    {
        await Clients.All.SendAsync("ReceiveCampaign", campaign);
    }
}
