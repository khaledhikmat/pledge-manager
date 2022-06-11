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
        private HttpClient _http;
        private ILogger<EntitiesService> _logger;
        private ISettingService _config;

        public EntitiesService(HttpClient http, ILogger<EntitiesService> logger, ISettingService config) 
        {
            _http = http;
            _logger = logger;
            _config = config;

            _http.BaseAddress = new Uri(_config.GetBackendBaseUrl());
        }
        
        public async Task<List<Campaign>> GetCampaigns() 
        {
            var requestUri = $"/entities/campaigns";
            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            request.Headers.Add("Accept", "application/json");
            _logger.LogInformation($"GetCampaigns = {request.RequestUri.ToString()}");

            var response = await _http.SendAsync(request);
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

            var response = await _http.SendAsync(request);
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

        public async Task<string> PostPledge(string campaignId, Pledge pledge)
        {
            var requestUri = $"/entities/campaigns/{campaignId}/pledges";
            var request = new HttpRequestMessage(HttpMethod.Post, requestUri);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            request.Content = new StringContent(JsonConvert.SerializeObject(pledge), Encoding.UTF8, "application/json");
            var response = await _http.SendAsync(request);

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
    }
}