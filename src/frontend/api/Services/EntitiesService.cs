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
        //WARNING: The http factories are no longer needed since we r using DAPR.
        //They are kept for refernce
        private IHttpClientFactory _campaignsHttpFactory;
        private IHttpClientFactory _usersHttpFactory;
        private HttpClient _campaignsHttpClient;
        private HttpClient _usersHttpClient;
        private ILogger<EntitiesService> _logger;
        private ISettingService _config;

        public EntitiesService(DaprClient daprClient, IHttpClientFactory campaignsHttp, IHttpClientFactory usersHttp, ILogger<EntitiesService> logger, ISettingService config) 
        {
            _daprClient = daprClient;
            _campaignsHttpFactory = campaignsHttp;
            _campaignsHttpClient = _campaignsHttpFactory.CreateClient("campaignsbackend");
            _usersHttpFactory = usersHttp;
            _usersHttpClient = _campaignsHttpFactory.CreateClient("usersbackend");
            _logger = logger;
            _config = config;

            _campaignsHttpClient.BaseAddress = new Uri(_config.GetCampaignsBackendBaseUrl());
            _usersHttpClient.BaseAddress = new Uri(_config.GetUsersBackendBaseUrl());
        }
        
        public async Task<List<Campaign>> GetCampaigns() 
        {
            var campaigns = await _daprClient.InvokeMethodAsync<List<Campaign>>(
            HttpMethod.Get,
            Constants.DAPR_CAMPAIGNS_APP_NAME,
            $"/entities/campaigns");

            return campaigns;
        }

        public async Task<Campaign> GetCampaign(string id) 
        {
            var campaign = await _daprClient.InvokeMethodAsync<Campaign>(
            HttpMethod.Get,
            Constants.DAPR_CAMPAIGNS_APP_NAME,
            $"/entities/campaigns/{id}");

            return campaign;
        }

        public async Task<List<FundSinkPeriod>> GetCampaignPeriods(string id) 
        {
            var periods = await _daprClient.InvokeMethodAsync<List<FundSinkPeriod>>(
            HttpMethod.Get,
            Constants.DAPR_CAMPAIGNS_APP_NAME,
            $"/entities/campaigns/{id}/periods");

            return periods;
        }

        public async Task<string> CommandCampaign(string campaignId, CampaignCommand command)
        {
            var response = await _daprClient.InvokeMethodAsync<CampaignCommand, ConfirmationResponse>(
            HttpMethod.Put,
            Constants.DAPR_CAMPAIGNS_APP_NAME,
            $"/entities/campaigns/{campaignId}/commands",
            command);

            return response != null ? response.Confirmation : "";
        }

        public async Task<string> UpdateCampaign(string campaignId, Campaign campaign)
        {
            var response = await _daprClient.InvokeMethodAsync<Campaign, ConfirmationResponse>(
            HttpMethod.Put,
            Constants.DAPR_CAMPAIGNS_APP_NAME,
            $"/entities/campaigns/{campaignId}/updates",
            campaign);

            return response != null ? response.Confirmation : "";
        }

        public async Task<string> PostPledge(string campaignId, Pledge pledge)
        {
            var response = await _daprClient.InvokeMethodAsync<Pledge, ConfirmationResponse>(
            HttpMethod.Post,
            Constants.DAPR_CAMPAIGNS_APP_NAME,
            $"/entities/campaigns/{campaignId}/pledges",
            pledge);

            return response != null ? response.Confirmation : "";
        }

        public async Task RegisterUser(string userName)
        {
            await _daprClient.InvokeMethodAsync(
            Constants.DAPR_USERS_APP_NAME,
            $"/users/verifications/{userName}");
        }

        public async Task VerifyUser(string userName, string code)
        {
            await _daprClient.InvokeMethodAsync(
            Constants.DAPR_USERS_APP_NAME,
            $"/users/verifications/{userName}/{code}");
        }
    }
}