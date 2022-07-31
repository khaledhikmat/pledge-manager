namespace pledgemanager.backend.campaigns.Controllers;

[ApiController]
[Route("[controller]")]
public class ProcessorController : ControllerBase
{
    private readonly ILogger<ProcessorController> _logger;
    private readonly IPersistenceService _persistenceService;

    public ProcessorController(ILogger<ProcessorController> logger, IPersistenceService persService)
    {
        _logger = logger;
        _persistenceService = persService;
    }

    //WARNING: The PUBSUB name is hard-coded!!!
    [Topic(Constants.DAPR_PUBSUB_NAME, Constants.DAPR_CAMPAIGN_COMMANDS_PROCESSOR_PUBSUB_TOPIC_NAME)]
    [Route("campaigncommandsprocessor")]
    [HttpPost()]
    public async Task<ActionResult> ProcessCampaignCommandsAsync(CampaignCommand command)
    {
        try
        {
            if (command == null || string.IsNullOrEmpty(command.Command))
            {
                throw new Exception("Command is null!");
            }

            _logger.LogInformation($"ProcessCampaignCommandsAsync - {command.CampaignIdentifier}/{command.Command}");

            var actorId = new ActorId(command.CampaignIdentifier);
            var proxy = ActorProxy.Create<ICampaignActor>(actorId, nameof(CampaignActor));
            await proxy.Command(command);
            return Ok();
        }
        catch (Exception e)
        {
            _logger.LogError($"ProcessCampaignCommandsAsync - {command.CampaignIdentifier}/{command.Command}- Exception: " + e.Message);
            _logger.LogError($"ProcessCampaignCommandsAsync - {command.CampaignIdentifier}/{command.Command} - Inner Exception: " + e.InnerException);
            return StatusCode(500);
        }
    }

    //WARNING: The PUBSUB name is hard-coded!!!
    [Topic(Constants.DAPR_PUBSUB_NAME, Constants.DAPR_CAMPAIGN_PLEDGES_PROCESSOR_PUBSUB_TOPIC_NAME)]
    [Route("campaignpledgesprocessor")]
    [HttpPost()]
    public async Task<ActionResult> ProcessCampaignPledgeAsync(Pledge pledge)
    {
        try
        {
            _logger.LogInformation($"ProcessCampaignPledgeAsync - {pledge.CampaignIdentifier}");

            var actorId = new ActorId(pledge.CampaignIdentifier);
            var proxy = ActorProxy.Create<ICampaignActor>(actorId, nameof(CampaignActor));
            await proxy.Pledge(pledge);
            return Ok();
        }
        catch (Exception e)
        {
            _logger.LogError($"ProcessCampaignPledgeAsync - {pledge.CampaignIdentifier} - Exception: " + e.Message);
            _logger.LogError($"ProcessCampaignPledgeAsync - {pledge.CampaignIdentifier} - Inner Exception: " + e.InnerException);
            return StatusCode(500);
        }
    }

    //WARNING: The PUBSUB name is hard-coded!!!
    [Topic(Constants.DAPR_PUBSUB_NAME, Constants.DAPR_USERS_PERSISTOR_PUBSUB_TOPIC_NAME)]
    [Route("userspersistor")]
    [HttpPost()]
    public async Task<ActionResult> PersistUsersAsync(User user)
    {
        try
        {
            _logger.LogInformation($"PersistUsersAsync - {user.Identifier}");
            await _persistenceService.SaveUser(user);
            return Ok();
        }
        catch (Exception e)
        {
            _logger.LogError($"PersistUsersAsync - {user.Identifier} - Exception: " + e.Message);
            _logger.LogError($"PersistUsersAsync - {user.Identifier} - Inner Exception: " + e.InnerException);
            return StatusCode(500);
        }
    }

    //WARNING: The PUBSUB name is hard-coded!!!
    [Topic(Constants.DAPR_PUBSUB_NAME, Constants.DAPR_FUNDSINKS_PERSISTOR_PUBSUB_TOPIC_NAME)]
    [Route("fundsinkspersistor")]
    [HttpPost()]
    public async Task<ActionResult> PersistFundSinksAsync(FundSink fundsink)
    {
        try
        {
            _logger.LogInformation($"PersistFundSinksAsync - {fundsink.Identifier}");
            await _persistenceService.SaveFundSink(fundsink);
            return Ok();
        }
        catch (Exception e)
        {
            _logger.LogError($"PersistFundSinksAsync - {fundsink.Identifier} - Exception: " + e.Message);
            _logger.LogError($"PersistFundSinksAsync - {fundsink.Identifier} - Inner Exception: " + e.InnerException);
            return StatusCode(500);
        }
    }

    //WARNING: The PUBSUB name is hard-coded!!!
    [Topic(Constants.DAPR_PUBSUB_NAME, Constants.DAPR_INSTITUTIONS_PERSISTOR_PUBSUB_TOPIC_NAME)]
    [Route("institutionspersistor")]
    [HttpPost()]
    public async Task<ActionResult> PersistInstitutionsAsync(Institution institution)
    {
        try
        {
            _logger.LogInformation($"PersistInstitutionsAsync - {institution.Identifier}");
            await _persistenceService.SaveInstitution(institution);
            return Ok();
        }
        catch (Exception e)
        {
            _logger.LogError($"PersistInstitutionsAsync - {institution.Identifier} - Exception: " + e.Message);
            _logger.LogError($"PersistInstitutionsAsync - {institution.Identifier} - Inner Exception: " + e.InnerException);
            return StatusCode(500);
        }
    }

    //WARNING: The PUBSUB name is hard-coded!!!
    [Topic(Constants.DAPR_PUBSUB_NAME, Constants.DAPR_CAMPAIGNS_PERSISTOR_PUBSUB_TOPIC_NAME)]
    [Route("campaignspersistor")]
    [HttpPost()]
    public async Task<ActionResult> PersistCampaignsAsync(Campaign campaign)
    {
        try
        {
            _logger.LogInformation($"PersistCampaignsAsync - {campaign.Identifier}");
            await _persistenceService.SaveCampaign(campaign);
            return Ok();
        }
        catch (Exception e)
        {
            _logger.LogError($"PersistCampaignsAsync - {campaign.Identifier} - Exception: " + e.Message);
            _logger.LogError($"PersistCampaignsAsync - {campaign.Identifier} - Inner Exception: " + e.InnerException);
            return StatusCode(500);
        }
    }

    //WARNING: The PUBSUB name is hard-coded!!!
    [Topic(Constants.DAPR_PUBSUB_NAME, Constants.DAPR_CAMPAIGN_PLEDGES_PERSISTOR_PUBSUB_TOPIC_NAME)]
    [Route("campaignpledgespersistor")]
    [HttpPost()]
    public async Task<ActionResult> PersistCampaignPledgesAsync(Pledge pledge)
    {
        try
        {
            _logger.LogInformation($"PersistCampaignPledgesAsync - {pledge.Identifier}");
            await _persistenceService.SavePledge(pledge);
            return Ok();
        }
        catch (Exception e)
        {
            _logger.LogError($"PersistCampaignPledgesAsync - {pledge.Identifier} - Exception: " + e.Message);
            _logger.LogError($"PersistCampaignPledgesAsync - {pledge.Identifier} - Inner Exception: " + e.InnerException);
            return StatusCode(500);
        }
    }

    //WARNING: The PUBSUB name is hard-coded!!!
    [Topic(Constants.DAPR_PUBSUB_NAME, Constants.DAPR_CAMPAIGN_DONORS_PERSISTOR_PUBSUB_TOPIC_NAME)]
    [Route("campaigndonorspersistor")]
    [HttpPost()]
    public async Task<ActionResult> PersistCampaignDonorsAsync(Donor donor)
    {
        try
        {
            _logger.LogInformation($"PersistCampaignDonorsAsync - {donor.Identifier}");
            await _persistenceService.SaveDonor(donor);
            return Ok();
        }
        catch (Exception e)
        {
            _logger.LogError($"PersistCampaignDonorsAsync - {donor.Identifier} - Exception: " + e.Message);
            _logger.LogError($"PersistCampaignDonorsAsync - {donor.Identifier} - Inner Exception: " + e.InnerException);
            return StatusCode(500);
        }
    }
}
