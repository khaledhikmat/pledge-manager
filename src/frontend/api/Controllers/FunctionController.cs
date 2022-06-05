using pledgemanager.frontend.api.Services;

namespace pledgemanager.frontend.api.Controllers;

[ApiController]
[Route("[controller]")]
public class FunctionController : ControllerBase
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly SignalRRestService _signalRService;
    private readonly IEntitiesService _entitiesService;
    private readonly ILogger<FunctionController> _logger;

    public FunctionController(IHttpClientFactory httpClientFactory, SignalRRestService signalRService, IEntitiesService entitiesService, ILogger<FunctionController> logger)
    {
        _httpClientFactory = httpClientFactory;
        _signalRService = signalRService;
        _entitiesService = entitiesService;
        _logger = logger;
    }

    [HttpGet]
    public IEnumerable<SimpleMessage> Get()
    {
        return Enumerable.Range(1, 5).Select(index => new SimpleMessage
        {
            Name = $"{Random.Shared.Next(-20, 55)} - {DateTime.Now.AddDays(index)}",
            Message = Summaries[Random.Shared.Next(Summaries.Length)]
        })
        .ToArray();
    }

    [Route("campaigns")]
    [HttpGet]
    public async Task<ActionResult> GetCampaigns() 
    {
        try
        {
            List<Campaign> campaigns = await _entitiesService.GetCampaigns();
            return Ok(campaigns);
        }
        catch (Exception e)
        {
            _logger.LogError($"GetCampaigns - Exception: " + e.Message);
            return StatusCode(500);
        }
    }

    [Route("campaigns/{id}")]
    [HttpGet]
    public async Task<ActionResult> GetCampaign(string id)
    {
        try
        {
            return Ok(await _entitiesService.GetCampaign(id));
        }
        catch (Exception e)
        {
            _logger.LogError($"GetCampaign - Exception: " + e.Message);
            return StatusCode(500);
        }
    }


    [Route("campaigns/{id}/pledges")]
    [HttpPost()]
    public async Task<ActionResult> PledgeCampaign(string id, [FromBody] Pledge pledge)
    {
        try
        {
            return Ok(await _entitiesService.PostPledge(id, pledge));
        }
        catch (Exception e)
        {
            _logger.LogError($"PledgeCampaign - Exception: " + e.Message);
            return StatusCode(500);
        }
    }

    [Route("signalr")]
    [HttpPost()]
    public async Task<ActionResult> SendCampaign([FromBody] string ignore)
    {
        try
        {
            var campaign = new Campaign();
            campaign.Identifier = Guid.NewGuid().ToString();
            campaign.Title = Summaries[Random.Shared.Next(Summaries.Length)];
            campaign.PledgesCount = Random.Shared.Next(Summaries.Length);
            _logger.LogInformation($"SendCampaign - {campaign.Identifier}");
            campaign.CreatedTime = DateTime.Now;
            campaign.LastUpdatedTime = DateTime.Now;

            var payload = new PayloadMessage();
            payload.Target = "UpdateCampaign";
            payload.Arguments = new[] { campaign };
            await _signalRService.CallViaRest("CampaignHub", payload);
            return Ok();
        }
        catch (Exception e)
        {
            _logger.LogError($"SendCampaign - Exception: " + e.Message);
            return StatusCode(500);
        }
    }
}
