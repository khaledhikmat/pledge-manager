namespace pledgemanager.shared.Contracts;

using Dapr.Actors;
using shared.Models;

public interface ICampaignActor : IActor 
{
    public Task<List<FundSinkPeriod>> GetPeriods();
    public Task<string> Update(Campaign campaign);
    public Task Pledge(Pledge pledge); 
    public Task Command(CampaignCommand command);
}