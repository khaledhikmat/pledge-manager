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
    private readonly IEnvironmentService _envService;
    private readonly IPersistenceService _persistenceService;

    public EntitiesController(IHttpClientFactory httpClientFactory, ILogger<EntitiesController> logger, IEnvironmentService envService, IPersistenceService persService)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _envService = envService;
        _persistenceService = persService;
    }

    //**** PING
    [Route("ping")]
    [HttpGet()]
    public async Task<ActionResult> Ping([FromServices] DaprClient daprClient)
    {
        try
        {
            _logger.LogInformation($"Ping");
            return Ok("I am up!");
        }
        catch (Exception e)
        {
            _logger.LogError($"Ping - Exception: " + e.Message);
            _logger.LogError($"Ping - Inner Exception: " + e.InnerException);
            return StatusCode(500);
        }
    }

    //**** SAMPLE DATA
    [Route("sample")]
    [HttpPost()]
    public async Task<ActionResult> CreateSampleDataAsync([FromServices] DaprClient daprClient)
    {
        try
        {
            _logger.LogInformation($"CreateSampleDataAsync");
            await _persistenceService.LoadSampleData();
            return Ok();
        }
        catch (Exception e)
        {
            _logger.LogError($"CreateSampleDataAsync - Exception: " + e.Message);
            _logger.LogError($"CreateSampleDataAsync - Inner Exception: " + e.InnerException);
            return StatusCode(500);
        }
    }

    //**** CAMPAIGNS
    [Route("campaigns/{id}")]
    [HttpGet()]
    public async Task<ActionResult> GetCampaignById(string id, [FromServices] DaprClient daprClient)
    {
        try
        {
            _logger.LogInformation($"GetCampaignById - {id}");
            return Ok(await _persistenceService.RetrieveCampaignById(id));
        }
        catch (Exception e)
        {
            _logger.LogError($"GetCampaignById - {id} - Exception: " + e.Message);
            _logger.LogError($"GetCampaignById - {id} - Inner Exception: " + e.InnerException);
            return StatusCode(500);
        }
    }

    [Route("campaigns/{id}/periods")]
    [HttpGet()]
    public async Task<ActionResult> GetCampaignPeriodsById(string id, [FromServices] DaprClient daprClient)
    {
        try
        {
            _logger.LogInformation($"GetCampaignPeriodsById - {id}");
            var actorId = new ActorId(id);
            var proxy = ActorProxy.Create<ICampaignActor>(actorId, nameof(CampaignActor));
            return Ok(await proxy.GetPeriods());
        }
        catch (Exception e)
        {
            _logger.LogError($"GetCampaignPeriodsById - {id} - Exception: " + e.Message);
            _logger.LogError($"GetCampaignPeriodsById - {id} - Inner Exception: " + e.InnerException);
            return StatusCode(500);
        }
    }

    [Route("campaigns")]
    [HttpGet()]
    public async Task<ActionResult> GetAllCampaigns([FromServices] DaprClient daprClient)
    {
        try
        {
            _logger.LogInformation($"GetAllCampaigns");
            return Ok(await _persistenceService.RetrieveCampaigns());
        }
        catch (Exception e)
        {
            _logger.LogError($"GetAllCampaigns - Exception: " + e.Message);
            _logger.LogError($"GetAllCampaigns - Inner Exception: " + e.InnerException);
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
            await _persistenceService.PersistCampaign(campaign);
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
            var existingCampaign = await _persistenceService.RetrieveCampaignById(id);
            if (existingCampaign == null)
            {
                throw new Exception($"Campaign [{id}] does not exist!!!");
            }

            _logger.LogInformation($"UpdateCampaignAsync - {id}");
            var actorId = new ActorId(id);
            var proxy = ActorProxy.Create<ICampaignActor>(actorId, nameof(CampaignActor));
            await proxy.Update(campaign);
            return Ok(new ConfirmationResponse() {Confirmation = "Success", Error = ""});
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

            var actorId = new ActorId(command.UserName);
            var proxy = ActorProxy.Create<IUserActor>(actorId, nameof(UserActor));
            if (!await proxy.IsVerified()) 
            {
                throw new Exception($"User is not verified!!!");
            }

            var campaign = await _persistenceService.RetrieveCampaignById(id);
            command.CampaignIdentifier = campaign.Identifier;
            _logger.LogInformation($"CommandCampaignAsync - {command.Identifier}");
            await daprClient.PublishEventAsync(_envService.GetPubSubName(), Constants.DAPR_CAMPAIGN_COMMANDS_PROCESSOR_PUBSUB_TOPIC_NAME, command);
            return Ok(new ConfirmationResponse() {Confirmation = command.Confirmation, Error = ""});
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

            _logger.LogInformation($"SubmitPledgeAsync2 - {id}");
            var actorId = new ActorId(pledge.UserName);
            var proxy = ActorProxy.Create<IUserActor>(actorId, nameof(UserActor));
            if (!await proxy.IsVerified()) 
            {
                throw new Exception($"User is not verified!!!");
            }
            _logger.LogInformation($"SubmitPledgeAsync3 - {id}");

            Campaign campaign = await _persistenceService.RetrieveCampaignById(id);

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
            await daprClient.PublishEventAsync(_envService.GetPubSubName(), Constants.DAPR_CAMPAIGN_PLEDGES_PROCESSOR_PUBSUB_TOPIC_NAME, pledge);
            return Ok(new ConfirmationResponse() {Confirmation = pledge.Confirmation, Error = ""});
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
            return Ok(await _persistenceService.RetrieveInstitutionById(id));
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
            await _persistenceService.PersistInstitution(institution);
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
            return Ok(await _persistenceService.RetrieveFundSinkById(id));
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
            await _persistenceService.PersistFundSink(fundsink);
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
