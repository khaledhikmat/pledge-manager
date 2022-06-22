using Microsoft.AspNetCore.SignalR;

namespace pledgemanager.frontend.api.Hubs;

public class PledgeHub : Hub
{
    public async Task SendPledge(Pledge pledge)
    {
        await Clients.All.SendAsync("EmphasizeCampaign", pledge);
    }
}
