using System.Text;
using System.Text.Json;
using static System.Net.Mime.MediaTypeNames;

namespace pledgemanager.backend.campaigns.Controllers;

/*
In this Backend Controller, we always read from the state store. We don't access the actors 
directly. This is not because we can't, but just to not be cognizant of how the processors
are actually implemented.

It is assumed that the state store has 2 copies of each FundSink:
1. State store so it can be queried and filtered for FundSinks.
2. Actors store so it can maintain actor state. This is considered internal 
and therefore never queried directly.

It is also assumed that actors externalize their state to the state store to keep it updated.

The create campaign/institition API Endpoint merely creates state store for the fundsink.
A counterpart actor is created lazingly when the campaign receives a request.

The command, update and pledge API Endpoints create actors lazingly to process the request.

This entities controller can be broken to several controllers:
- Campaigngs
- Institutions
- Regions
- Pledges
- Donors
- Etc
*/

[ApiController]
[Route("[controller]")]
public class EntitiesController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<EntitiesController> _logger;

    public EntitiesController(IHttpClientFactory httpClientFactory, ILogger<EntitiesController> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    //**** CAMPAIGNS
    [Route("campaigns/{id}")]
    [HttpGet()]
    public async Task<ActionResult> GetCampaignById(string id, [FromServices] DaprClient daprClient)
    {
        try
        {
            _logger.LogInformation($"GetCampaignById - {id}");
            var stateEntry = await daprClient.GetStateEntryAsync<Campaign>(Constants.DAPR_CAMPAIGNS_STORE_NAME, id);
            return Ok(stateEntry != null ? stateEntry.Value : null);
        }
        catch (Exception e)
        {
            _logger.LogError($"GetCampaignById - {id} - Exception: " + e.Message);
            _logger.LogError($"GetCampaignById - {id} - Inner Exception: " + e.InnerException);
            return StatusCode(500);
        }
    }

    [Route("campaigns")]
    [HttpGet()]
    public async Task<ActionResult> GetAllCampaigns([FromServices] DaprClient daprClient)
    {
        return await QueryCampaigns(new CampaignQuery(), daprClient);
    }

    [Route("campaignsquery")]
    [HttpPost]
    public async Task<ActionResult> QueryCampaigns([FromBody]CampaignQuery query, [FromServices] DaprClient daprClient)
    {
        try
        {
            List<Campaign> campaigns = new List<Campaign>();
            // The options to query in DAPR state store are currently limited:
            // 1. daprClient.GetBulkStateAsync - requires the actual Ids
            // 2. https://docs.dapr.io/developing-applications/building-blocks/state-management/howto-state-query-api/
            //
            // The first option requires the actual ids to be provided!! The second option, however, is more 
            // powerful but does not have an SDK equivalent (as far as I can tell)
            _logger.LogInformation($"QueryCampaigns");

            if (0 == 1)
            {
                var httpClient = _httpClientFactory.CreateClient("dapr");

                var queryJson = new StringContent(
                    JsonSerializer.Serialize(query),
                    Encoding.UTF8,
                    Application.Json
                ); 
                _logger.LogInformation($"QueryCampaigns query: {JsonSerializer.Serialize(query)}");

                using var httpResponseMessage = await httpClient.PostAsync(
                    $"/v1.0-alpha1/state/{Constants.DAPR_CAMPAIGNS_STORE_NAME}/query?metadata.contentType=application/json", queryJson);

                _logger.LogInformation($"QueryCampaigns response: {httpResponseMessage.ToString()}");

                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    using var contentStream = await httpResponseMessage.Content.ReadAsStreamAsync();
                    campaigns = await JsonSerializer.DeserializeAsync<List<Campaign>>(contentStream);
                }
                else 
                {
                    throw new Exception($"Status code: {httpResponseMessage.StatusCode} - Reason: {httpResponseMessage.ReasonPhrase}");
                }
            }
            else 
            {
                //TODO: Hard-coded for now
                IReadOnlyList<BulkStateItem> mulitpleStateResult = await daprClient.GetBulkStateAsync(
                    Constants.DAPR_CAMPAIGNS_STORE_NAME, 
                    new List<string> { 
                        "CAMP-00001", 
                        "CAMP-00002", 
                        "CAMP-00003", 
                        "CAMP-00004", 
                        "CAMP-00005", 
                        "CAMP-00006", 
                        "CAMP-00007", 
                        "CAMP-00008", 
                        "CAMP-00009", 
                        "CAMP-00010" 
                    }, 
                    parallelism: 1);   
                List<BulkStateItem> items = new List<BulkStateItem>(mulitpleStateResult);

                var serializationOptions = new JsonSerializerOptions();
                serializationOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                foreach (BulkStateItem item in items) 
                {
                    if (!string.IsNullOrEmpty(item.Value)) 
                    {
                        _logger.LogInformation($"Deserializing...{item.Key} - value: {item.Value}");
                        var o = JsonSerializer.Deserialize<Campaign>(item.Value, serializationOptions);
                        if (o != null) 
                        {
                            campaigns.Add(o);
                        }
                    }
                }
            }

            return Ok(campaigns);
        }
        catch (Exception e)
        {
            _logger.LogError($"QueryCampaigns - Exception: " + e.Message);
            _logger.LogError($"QueryCampaigns - Inner Exception: " + e.InnerException);
            return StatusCode(500);
        }
    }

    [Route("campaigns")]
    [HttpPost()]
    public async Task<ActionResult> CreateCampaignAsync([FromBody]Campaign campaign, [FromServices] DaprClient daprClient)
    {
        try
        {
            _logger.LogInformation($"CreateCampaignAsync - {campaign.Identifier}");
            campaign.CreatedTime = DateTime.Now;
            campaign.LastUpdatedTime = campaign.CreatedTime;
            await daprClient.SaveStateAsync<Campaign>(Constants.DAPR_CAMPAIGNS_STORE_NAME, campaign.Identifier, campaign);
            return Ok();
        }
        catch (Exception e)
        {
            _logger.LogError($"CreateCampaignAsync - {campaign.Identifier} - Exception: " + e.Message);
            _logger.LogError($"CreateCampaignAsync - {campaign.Identifier} - Inner Exception: " + e.InnerException);
            return StatusCode(500);
        }
    }

    [Route("campaigns/{id}/updates")]
    [HttpPut()]
    public async Task<ActionResult> UpdateCampaignAsync(string id, [FromBody]Campaign campaign, [FromServices] DaprClient daprClient)
    {
        try
        {
            _logger.LogInformation($"UpdateCampaignAsync - {id}");
            var stateEntry = await daprClient.GetStateEntryAsync<Campaign>(Constants.DAPR_CAMPAIGNS_STORE_NAME, id);
            if (stateEntry == null || stateEntry.Value == null)
            {
                throw new Exception($"Campaign [{id}] does not exist!!!");
            }

            _logger.LogInformation($"UpdateCampaignAsync - {id}");
            var actorId = new ActorId(id);
            var proxy = ActorProxy.Create<ICampaignActor>(actorId, nameof(CampaignActor));
            await proxy.Update(campaign);
            return Ok("something");
        }
        catch (Exception e)
        {
            _logger.LogError($"UpdateCampaignAsync - {id} - Exception: " + e.Message);
            _logger.LogError($"UpdateCampaignAsync - {id} - Inner Exception: " + e.InnerException);
            return StatusCode(500);
        }
    }

    [Route("campaigns/{id}/commands")]
    [HttpPut()]
    public async Task<ActionResult> CommandCampaignAsync(string id, [FromBody] CampaignCommand command, [FromServices] DaprClient daprClient)
    {
        try
        {
            _logger.LogInformation($"CommandCampaignAsync - {id}");
            if (command == null)
            {
                throw new Exception($"Command is NULL!!!");
            }

            if (string.IsNullOrEmpty(command.UserName)) 
            {
                throw new Exception($"Command username must exist!!!");
            }

            //Validate pledge username using the users microservice
            //The users microervice might respond with a 401 to indicate non-valid
            await daprClient.InvokeMethodAsync("pledgemanager-users", $"users/verifications/{command.UserName}");    

            var stateEntry = await daprClient.GetStateEntryAsync<Campaign>(Constants.DAPR_CAMPAIGNS_STORE_NAME, id);
            if (stateEntry == null || stateEntry.Value == null)
            {
                throw new Exception($"Campain [{id}] does not exist!!!");
            }

            Campaign campaign = stateEntry.Value;
            command.CampaignIdentifier = campaign.Identifier;
            _logger.LogInformation($"CommandCampaignAsync - {command.Identifier}");
            await daprClient.PublishEventAsync(Constants.DAPR_CAMPAIGNS_PUBSUB_NAME, Constants.DAPR_COMMANDS_PUBSUB_TOPIC_NAME, command);
            return Ok(command.Confirmation);
        }
        catch (Exception e)
        {
            _logger.LogError($"CommandCampaignAsync - {id} - cmd: {command.Identifier} - Exception: " + e.Message);
            _logger.LogError($"CommandCampaignAsync - {id} - cmd: {command.Identifier} - Inner Exception: " + e.InnerException);
            return StatusCode(500);
        }
    }

    [Route("campaigns/{id}/pledges")]
    [HttpPost()]
    public async Task<ActionResult> SubmitPledgeAsync(string id, [FromBody]Pledge pledge, [FromServices] DaprClient daprClient)
    {
        try
        {
            _logger.LogInformation($"SubmitPledgeAsync - {id}");
            if (pledge == null)
            {
                throw new Exception($"Pledge is null!!!");
            }

            if (string.IsNullOrEmpty(pledge.UserName))
            {
                throw new Exception($"Pledge userName is null!!!");
            }

            //Validate pledge username using the users microservice
            //The users microervice might respond with a 401 to indicate non-valid
            await daprClient.InvokeMethodAsync("pledgemanager-users", $"users/verifications/{pledge.UserName}");    

            var stateEntry = await daprClient.GetStateEntryAsync<Campaign>(Constants.DAPR_CAMPAIGNS_STORE_NAME, id);
            if (stateEntry == null || stateEntry.Value == null)
            {
                throw new Exception($"Campaign [{id}] does not exist!!!");
            }

            Campaign campaign = stateEntry.Value;
            if (!campaign.IsActive)
            {
                throw new Exception($"Campaign [{id}] is not active!!!");
            }

            if (pledge.Amount < campaign.Behavior.MinPledgeAmount ||
                pledge.Amount > campaign.Behavior.MaxPledgeAmount) 
            {
                throw new Exception($"Pledge amount {pledge.Amount} is less than min or larger than max.");
            }

            if (campaign.Behavior.RestrictedPledgeAmounts.Count > 0 && 
                !campaign.Behavior.RestrictedPledgeAmounts.Contains(pledge.Amount))
            {
                throw new Exception($"Pledge amount {pledge.Amount} is not acceptable.");
            }

            pledge.CampaignIdentifier = campaign.Identifier;
            _logger.LogInformation($"SubmitPledgeAsync - {pledge.Identifier}");
            await daprClient.PublishEventAsync(Constants.DAPR_CAMPAIGNS_PUBSUB_NAME, Constants.DAPR_PLEDGES_PUBSUB_TOPIC_NAME, pledge);
            return Ok(pledge.Confirmation);
        }
        catch (Exception e)
        {
            _logger.LogError($"SubmitPledgeAsync - {pledge.Identifier} - Exception: " + e.Message);
            _logger.LogError($"SubmitPledgeAsync - {pledge.Identifier} - Inner Exception: " + e.InnerException);
            return StatusCode(500);
        }
    }

    //**** INSTITUTIONS
    [Route("institutions/{id}")]
    [HttpGet()]
    public async Task<ActionResult> GetInstitutionById(string id, [FromServices] DaprClient daprClient)
    {
        try
        {
            _logger.LogInformation($"GetInstitutionById - {id}");
            var stateEntry = await daprClient.GetStateEntryAsync<Institution>(Constants.DAPR_CAMPAIGNS_STORE_NAME, id);
            return Ok(stateEntry != null ? stateEntry.Value : null);
        }
        catch (Exception e)
        {
            _logger.LogError($"GetInstitutionById - {id} - Exception: " + e.Message);
            _logger.LogError($"GetInstitutionById - {id} - Inner Exception: " + e.InnerException);
            return StatusCode(500);
        }
    }

    [Route("institutions")]
    [HttpPost()]
    public async Task<ActionResult> CreateInstitutionAsync([FromBody]Institution institution, [FromServices] DaprClient daprClient)
    {
        try
        {
            _logger.LogInformation($"CreateInstitutionAsync - {institution.Identifier}");
            institution.CreatedTime = DateTime.Now;
            institution.LastUpdatedTime = institution.CreatedTime;
            await daprClient.SaveStateAsync<Institution>(Constants.DAPR_CAMPAIGNS_STORE_NAME, institution.Identifier, institution);
            return Ok();
        }
        catch (Exception e)
        {
            _logger.LogError($"CreateInstitutionAsync - {institution.Identifier} - Exception: " + e.Message);
            _logger.LogError($"CreateInstitutionAsync - {institution.Identifier} - Inner Exception: " + e.InnerException);
            return StatusCode(500);
        }
    }

    //**** FUNDSINKS
    [Route("fundsinks/{id}")]
    [HttpGet()]
    public async Task<ActionResult> GetFundSinkById(string id, [FromServices] DaprClient daprClient)
    {
        try
        {
            _logger.LogInformation($"GetFundSinkById - {id}");
            var stateEntry = await daprClient.GetStateEntryAsync<FundSink>(Constants.DAPR_CAMPAIGNS_STORE_NAME, id);
            return Ok(stateEntry != null ? stateEntry.Value : null);
        }
        catch (Exception e)
        {
            _logger.LogError($"GetFundSinkById - {id} - Exception: " + e.Message);
            _logger.LogError($"GetFundSinkById - {id} - Inner Exception: " + e.InnerException);
            return StatusCode(500);
        }
    }

    [Route("fundsinks")]
    [HttpPost()]
    public async Task<ActionResult> CreateFundSinkAsync([FromBody]FundSink fundsink, [FromServices] DaprClient daprClient)
    {
        try
        {
            _logger.LogInformation($"CreateFundSinkAsync - {fundsink.Identifier}");
            fundsink.CreatedTime = DateTime.Now;
            fundsink.LastUpdatedTime = fundsink.CreatedTime;
            await daprClient.SaveStateAsync<FundSink>(Constants.DAPR_CAMPAIGNS_STORE_NAME, fundsink.Identifier, fundsink);
            return Ok();
        }
        catch (Exception e)
        {
            _logger.LogError($"CreateFundSinkAsync - {fundsink.Identifier} - Exception: " + e.Message);
            _logger.LogError($"CreateFundSinkAsync - {fundsink.Identifier} - Inner Exception: " + e.InnerException);
            return StatusCode(500);
        }
    }
}
