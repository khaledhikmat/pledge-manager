using System.Net.Http.Json;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;

using pledgemanager.shared.Models;
using System.Text;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using Dapr.Client;
using pledgemanager.shared.Utils;

namespace pledgemanager.frontend.api.Services
{
    public class EntitiesService : IEntitiesService 
    {
        private DaprClient _daprClient;
        private ILogger<EntitiesService> _logger;
        private IEnvironmentService _envService;

        public EntitiesService(DaprClient daprClient, ILogger<EntitiesService> logger, IEnvironmentService envService) 
        {
            _daprClient = daprClient;
            _logger = logger;
            _envService = envService;
        }
        
        public async Task<List<Campaign>> GetCampaigns() 
        {
            var campaigns = await _daprClient.InvokeMethodAsync<List<Campaign>>(
            HttpMethod.Get,
            _envService.GetCampaignsAppName(),
            $"/entities/campaigns");

            return campaigns;
        }

        public async Task<Campaign> GetCampaign(string id) 
        {
            var campaign = await _daprClient.InvokeMethodAsync<Campaign>(
            HttpMethod.Get,
            _envService.GetCampaignsAppName(),
            $"/entities/campaigns/{id}");

            return campaign;
        }

        public async Task<List<FundSinkPeriod>> GetCampaignPeriods(string id) 
        {
            var periods = await _daprClient.InvokeMethodAsync<List<FundSinkPeriod>>(
            HttpMethod.Get,
            _envService.GetCampaignsAppName(),
            $"/entities/campaigns/{id}/periods");

            return periods;
        }

        public async Task<string> CommandCampaign(string campaignId, CampaignCommand command)
        {
            var response = await _daprClient.InvokeMethodAsync<CampaignCommand, ConfirmationResponse>(
            HttpMethod.Put,
            _envService.GetCampaignsAppName(),
            $"/entities/campaigns/{campaignId}/commands",
            command);

            return response != null ? response.Confirmation : "";
        }

        public async Task<string> UpdateCampaign(string campaignId, Campaign campaign)
        {
            var response = await _daprClient.InvokeMethodAsync<Campaign, ConfirmationResponse>(
            HttpMethod.Put,
            _envService.GetCampaignsAppName(),
            $"/entities/campaigns/{campaignId}/updates",
            campaign);

            return response != null ? response.Confirmation : "";
        }

        public async Task<string> PostPledge(string campaignId, Pledge pledge)
        {
            var response = await _daprClient.InvokeMethodAsync<Pledge, ConfirmationResponse>(
            HttpMethod.Post,
            _envService.GetCampaignsAppName(),
            $"/entities/campaigns/{campaignId}/pledges",
            pledge);

            return response != null ? response.Confirmation : "";
        }

        public async Task RegisterUser(string userName)
        {
            await _daprClient.InvokeMethodAsync(
            _envService.GetUsersAppName(),
            $"/users/verifications/{userName}");
        }

        public async Task VerifyUser(string userName, string code)
        {
            await _daprClient.InvokeMethodAsync(
            _envService.GetUsersAppName(),
            $"/users/verifications/{userName}/{code}");
        }
    }
}