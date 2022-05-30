using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

using pledgemanager.shared.Models;
using pledgemanager.frontend.api.Services;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace pledgemanager.frontend.api.Functions
{
    public class EntitiesFunctionApp
    {
        private readonly ILogger _logger;
        private readonly HttpClient _http;
        private readonly IEntitiesService _entitiesService;

        public EntitiesFunctionApp(ILoggerFactory loggerFactory, IHttpClientFactory httpClientFactory, IEntitiesService service)
        {
            _logger = loggerFactory.CreateLogger<EntitiesFunctionApp>();
            _http = httpClientFactory.CreateClient();
            _entitiesService = service;
        }

        [Function("campaigns")]
        public async Task<HttpResponseData> RunGetCampaigns([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "campaigns")] 
            HttpRequestData req,
            FunctionContext context)
        {
            var logger = context.GetLogger("EntitiesFunctionApp");
            logger.LogInformation("RunGetCampaigns started....");

            List<Campaign> campaigns = await _entitiesService.GetCampaigns();
            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(campaigns);
            return response;
        }

        [Function("campaign")]
        public async Task<HttpResponseData> RunGetCampaign([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "campaigns/{id}")] 
            HttpRequestData req, 
            string id,
            FunctionContext context)
        {
            var logger = context.GetLogger("EntitiesFunctionApp");
            logger.LogInformation("RunGetCampaign started....");

            Campaign campaign = await _entitiesService.GetCampaign(id);
            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteAsJsonAsync(campaign);
            return response;
        }
    }
}
