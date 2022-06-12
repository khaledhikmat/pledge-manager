using System.Collections.Generic;
using System.Threading.Tasks;
using pledgemanager.shared.Models;

namespace pledgemanager.frontend.api.Services
{
    public interface IEntitiesService 
    {
        Task<List<Campaign>> GetCampaigns();
        Task<Campaign> GetCampaign(string id);
        Task<string> UpdateCampaign(string campaignId, Campaign campaign);
        Task<string> CommandCampaign(string campaignId, CampaignCommand command);
        Task<string> PostPledge(string campaignId, Pledge pledge);
    }
}