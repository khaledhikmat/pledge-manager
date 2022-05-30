namespace pledgemanager.backend.processors.Controllers;

[ApiController]
[Route("[controller]")]
public class ProcessorController : ControllerBase
{
    private readonly ILogger<ProcessorController> _logger;
    public ProcessorController(ILogger<ProcessorController> logger)
    {
        _logger = logger;
    }

    [Topic(Constants.DAPR_PUBSUB_NAME, Constants.DAPR_CAMPAIGNS_PUBSUB_TOPIC_NAME)]
    [Route("campaigns")]
    [HttpPost()]
    public async Task<ActionResult> UpdateCampaignAsync(Campaign campaign)
    {
        try
        {
            _logger.LogInformation($"UpdateCampaignAsync - {campaign.Identifier}");

            var actorId = new ActorId(campaign.Identifier);
            var proxy = ActorProxy.Create<ICampaignActor>(actorId, nameof(CampaignActor));
            await proxy.Update(campaign);
            return Ok();
        }
        catch (Exception e)
        {
            _logger.LogError($"UpdateCampaignAsync - {campaign.Identifier} - Exception: " + e.Message);
            _logger.LogError($"UpdateCampaignAsync - {campaign.Identifier} - Inner Exception: " + e.InnerException);
            return StatusCode(500);
        }
    }

    [Topic(Constants.DAPR_PUBSUB_NAME, Constants.DAPR_COMMANDS_PUBSUB_TOPIC_NAME)]
    [Route("campaigncommands")]
    [HttpPost()]
    public async Task<ActionResult> IssueCampaignCommandAsync(CampaignCommand command)
    {
        try
        {
            if (command == null || string.IsNullOrEmpty(command.Command))
            {
                throw new Exception("Command is null!");
            }

            _logger.LogInformation($"IssueCampaignCommandAsync - {command.CampaignIdentifier}/{command.Command}");

            var actorId = new ActorId(command.CampaignIdentifier);
            var proxy = ActorProxy.Create<ICampaignActor>(actorId, nameof(CampaignActor));
            await proxy.Command(command);
            return Ok();
        }
        catch (Exception e)
        {
            _logger.LogError($"IssueCampaignCommandAsync - {command.CampaignIdentifier}/{command.Command}- Exception: " + e.Message);
            _logger.LogError($"IssueCampaignCommandAsync - {command.CampaignIdentifier}/{command.Command} - Inner Exception: " + e.InnerException);
            return StatusCode(500);
        }
    }

    [Topic(Constants.DAPR_PUBSUB_NAME, Constants.DAPR_PLEDGES_PUBSUB_TOPIC_NAME)]
    [Route("pledges")]
    [HttpPost()]
    public async Task<ActionResult> ProcessPledgeAsync(Pledge pledge)
    {
        try
        {
            _logger.LogInformation($"ProcessPledgeAsync - {pledge.CampaignIdentifier}");

            var actorId = new ActorId(pledge.CampaignIdentifier);
            var proxy = ActorProxy.Create<ICampaignActor>(actorId, nameof(CampaignActor));
            await proxy.Pledge(pledge);
            return Ok();
        }
        catch (Exception e)
        {
            _logger.LogError($"ProcessPledgeAsync - {pledge.CampaignIdentifier} - Exception: " + e.Message);
            _logger.LogError($"ProcessPledgeAsync - {pledge.CampaignIdentifier} - Inner Exception: " + e.InnerException);
            return StatusCode(500);
        }
    }
}
