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

namespace pledgemanager.frontend.api.Services
{
    public class EntitiesService : IEntitiesService 
    {
        private IHttpClientFactory _campaignsHttpFactory;
        private IHttpClientFactory _usersHttpFactory;
        private HttpClient _campaignsHttpClient;
        private HttpClient _usersHttpClient;
        private ILogger<EntitiesService> _logger;
        private ISettingService _config;

        public EntitiesService(IHttpClientFactory campaignsHttp, IHttpClientFactory usersHttp, ILogger<EntitiesService> logger, ISettingService config) 
        {
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
            var requestUri = $"/entities/campaigns";
            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            request.Headers.Add("Accept", "application/json");
            _logger.LogInformation($"GetCampaigns = {request.RequestUri.ToString()}");

            var response = await _campaignsHttpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode) 
            {
                throw new ApplicationException($"{response.StatusCode}-{response.ReasonPhrase}");
            }

            var campaigns = await response.Content.ReadFromJsonAsync<List<Campaign>>();
            if (campaigns == null)
            {
                _logger.LogWarning($"GetCampaigns returned no campaigns!!!");
                return new List<Campaign>();
            }

            return campaigns;
        }

        public async Task<Campaign> GetCampaign(string id) 
        {
            var requestUri = $"/entities/campaigns/{id}";
            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            request.Headers.Add("Accept", "application/json");
            _logger.LogInformation($"GetCampaign = {request.RequestUri.ToString()}");

            var response = await _campaignsHttpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode) 
            {
                throw new ApplicationException($"{response.StatusCode}-{response.ReasonPhrase}");
            }

            var campaign = await response.Content.ReadFromJsonAsync<Campaign>();
            if (campaign == null) 
            {
                _logger.LogWarning($"GetCampaign/{id} returned no campaign!!!");
                return new Campaign();
            }

            return campaign;
        }

        public async Task<List<FundSinkPeriod>> GetCampaignPeriods(string id) 
        {
            var requestUri = $"/entities/campaigns/{id}/periods";
            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            request.Headers.Add("Accept", "application/json");
            _logger.LogInformation($"GetCampaignPeriods = {request.RequestUri.ToString()}");

            var response = await _campaignsHttpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode) 
            {
                throw new ApplicationException($"{response.StatusCode}-{response.ReasonPhrase}");
            }

            var periods = await response.Content.ReadFromJsonAsync<List<FundSinkPeriod>>();
            if (periods == null) 
            {
                _logger.LogWarning($"GetCampaignPeriods/{id} returned no periods!!!");
                return new List<FundSinkPeriod>();
            }

            return periods;
        }

        public async Task<string> CommandCampaign(string campaignId, CampaignCommand command)
        {
            var requestUri = $"/entities/campaigns/{campaignId}/commands";
            var request = new HttpRequestMessage(HttpMethod.Put, requestUri);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Content = new StringContent(JsonConvert.SerializeObject(command), Encoding.UTF8, "application/json");
            var response = await _campaignsHttpClient.SendAsync(request);

            if (response == null) 
            {
                throw new ApplicationException($"Null response from API Endpoint!");
            }

            if (!response.IsSuccessStatusCode) 
            {
                throw new ApplicationException($"{response.StatusCode}-{response.ReasonPhrase}");
            }

            //return Guid.NewGuid().ToString();
            return await response.Content.ReadFromJsonAsync<string>();
        }

        public async Task<string> UpdateCampaign(string campaignId, Campaign campaign)
        {
            var requestUri = $"/entities/campaigns/{campaignId}/updates";
            var request = new HttpRequestMessage(HttpMethod.Put, requestUri);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Content = new StringContent(JsonConvert.SerializeObject(campaign), Encoding.UTF8, "application/json");
            var response = await _campaignsHttpClient.SendAsync(request);

            if (response == null) 
            {
                throw new ApplicationException($"Null response from API Endpoint!");
            }

            if (!response.IsSuccessStatusCode) 
            {
                throw new ApplicationException($"{response.StatusCode}-{response.ReasonPhrase}");
            }

            //return Guid.NewGuid().ToString();
            return await response.Content.ReadFromJsonAsync<string>();
        }

        public async Task<string> PostPledge(string campaignId, Pledge pledge)
        {
            var requestUri = $"/entities/campaigns/{campaignId}/pledges";
            var request = new HttpRequestMessage(HttpMethod.Post, requestUri);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Content = new StringContent(JsonConvert.SerializeObject(pledge), Encoding.UTF8, "application/json");
            var response = await _campaignsHttpClient.SendAsync(request);

            if (response == null) 
            {
                throw new ApplicationException($"Null response from API Endpoint!");
            }

            if (!response.IsSuccessStatusCode) 
            {
                throw new ApplicationException($"{response.StatusCode}-{response.ReasonPhrase}");
            }

            //return Guid.NewGuid().ToString();
            return await response.Content.ReadFromJsonAsync<string>();
        }

        public async Task RegisterUser(string userName)
        {
            var requestUri = $"/users/verifications/{userName}";
            var request = new HttpRequestMessage(HttpMethod.Post, requestUri);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var response = await _usersHttpClient.SendAsync(request);

            if (response == null) 
            {
                throw new ApplicationException($"Null response from API Endpoint!");
            }

            if (!response.IsSuccessStatusCode) 
            {
                throw new ApplicationException($"{response.StatusCode}-{response.ReasonPhrase}");
            }
        }

        public async Task VerifyUser(string userName, string code)
        {
            var requestUri = $"/users/verifications/{userName}/{code}";
            var request = new HttpRequestMessage(HttpMethod.Post, requestUri);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var response = await _usersHttpClient.SendAsync(request);

            if (response == null) 
            {
                throw new ApplicationException($"Null response from API Endpoint!");
            }

            if (!response.IsSuccessStatusCode) 
            {
                throw new ApplicationException($"{response.StatusCode}-{response.ReasonPhrase}");
            }
        }
    }
}