using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Dapr.Client;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.IdentityModel.Tokens;
using pledgemanager.frontend.api.Services;

namespace pledgemanager.frontend.api.Controllers;

[ApiController]
[EnableCors(Constants.BLAZOR_POLICY)]
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
    private readonly IConfiguration _configuration;

    public FunctionController(
        IHttpClientFactory httpClientFactory, 
        SignalRRestService signalRService, 
        IEntitiesService entitiesService, 
        ILogger<FunctionController> logger, 
        IConfiguration config)
    {
        _httpClientFactory = httpClientFactory;
        _signalRService = signalRService;
        _entitiesService = entitiesService;
        _logger = logger;
        _configuration = config;
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

    [Authorize]
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

    [Authorize]
    [Route("campaigns/{id}/periods")]
    [HttpGet]
    public async Task<ActionResult> GetCampaignPeriods(string id)
    {
        try
        {
            return Ok(await _entitiesService.GetCampaignPeriods(id));
        }
        catch (Exception e)
        {
            _logger.LogError($"GetCampaignPeriods - Exception: " + e.Message);
            return StatusCode(500);
        }
    }

    [Authorize]
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
            _logger.LogError($"PledgeCampaign - Exception: {e.Message} - {e.InnerException}");
            return StatusCode(500);
        }
    }

    [Authorize]
    [Route("campaigns/{id}/commands")]
    [HttpPost()]
    public async Task<ActionResult> CommandCampaign(string id, [FromBody] CampaignCommand command)
    {
        try
        {
            return Ok(await _entitiesService.CommandCampaign(id, command));
        }
        catch (Exception e)
        {
            _logger.LogError($"CommandCampaign - Exception: " + e.Message);
            return StatusCode(500);
        }
    }

    [Authorize]
    [Route("campaigns/{id}/updates")]
    [HttpPost()]
    public async Task<ActionResult> UpdateCampaign(string id, [FromBody] Campaign campaign)
    {
        try
        {
            _logger.LogInformation($"UpdateCampaign - {id} - {campaign.Behavior.PledgeMode}");
            return Ok(await _entitiesService.UpdateCampaign(id, campaign));
        }
        catch (Exception e)
        {
            _logger.LogError($"UpdateCampaign - Exception: " + e.Message);
            return StatusCode(500);
        }
    }

    [Route("users/registrations/{username}")]
    [HttpPost()]
    public async Task<ActionResult> RegisterUser(string username)
    {
        try
        {
            _logger.LogInformation($"RegisterUser - {username}");
            await _entitiesService.RegisterUser(username);
            return Ok();
        }
        catch (Exception e)
        {
            _logger.LogError($"RegisterUser - Exception: " + e.Message);
            return StatusCode(500);
        }
    }

    [Route("users/verifications/{username}/{code}")]
    [HttpPost()]
    public async Task<ActionResult> VerifyUser(string username, string code)
    {
        try
        {
            _logger.LogInformation($"VerifyUser - {username}");
            var verifyResponse = await _entitiesService.VerifyUser(username, code);
            if (verifyResponse == null)
            {
                throw new Exception("Verify reponse is empty!!");
            }

            var claims = new List<Claim>();
            claims.Add(new Claim(ClaimTypes.Name, username));

            if (verifyResponse.Type == UserTypes.Organizer)
            {
                claims.Add(new Claim(ClaimTypes.Role, "Organizer"));
                //TODO: Deal with permitted institutions here
            }
            else if (verifyResponse.Type == UserTypes.Admin)
            {
                claims.Add(new Claim(ClaimTypes.Role, "Admin"));
            }
            else if (verifyResponse.Type == UserTypes.Donor)
            {
                claims.Add(new Claim(ClaimTypes.Role, "Donor"));
            }
            else
            {
                throw new Exception("Unknown user type");
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSecurityKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expiry = DateTime.Now.AddDays(Convert.ToInt32(_configuration["JwtExpiryInDays"]));

            var token = new JwtSecurityToken(
                _configuration["JwtIssuer"],
                _configuration["JwtAudience"],
                claims,
                expires: expiry,
                signingCredentials: creds
            );

            return Ok(new LoginResult { Successful = true, Token = new JwtSecurityTokenHandler().WriteToken(token) });
        }
        catch (Exception e)
        {
            _logger.LogError($"VerifyUser - Exception: " + e.Message);
            return StatusCode(500);
        }
    }

    [Authorize]
    [Route("signalr/campaigns")]
    [HttpPost()]
    public async Task<ActionResult> SendCampaign([FromBody] Campaign campaign)
    {
        try
        {
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

    [Authorize]
    [Route("signalr/pledges")]
    [HttpPost()]
    public async Task<ActionResult> SendPledge([FromBody] Pledge pledge)
    {
        try
        {
            var payload = new PayloadMessage();
            payload.Target = "EmphasizePledge";
            payload.Arguments = new[] { pledge };
            await _signalRService.CallViaRest("PledgeHub", payload);
            return Ok();
        }
        catch (Exception e)
        {
            _logger.LogError($"SendPledge - Exception: " + e.Message);
            return StatusCode(500);
        }
    }
}
