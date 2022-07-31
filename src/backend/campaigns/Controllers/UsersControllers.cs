using Microsoft.AspNetCore.Mvc;

namespace pledgemanager.backend.campaigns.Controllers;

[ApiController]
[Route("[controller]")]
public class UsersController : ControllerBase
{
    private readonly ILogger<UsersController> _logger;
    private readonly IEnvironmentService _envService;
    private readonly IPersistenceService _persistenceService;

    public UsersController(ILogger<UsersController> logger, IEnvironmentService envService, IPersistenceService persService)
    {
        _logger = logger;
        _envService = envService;
        _persistenceService = persService;
    }

    //**** USERS
    [Route("{id}")]
    [HttpGet()]
    public async Task<ActionResult> GetUserById(string id, [FromServices] DaprClient daprClient)
    {
        try
        {
            _logger.LogInformation($"GetUserById - {id}");
            return Ok(await _persistenceService.RetrieveUserById(id));
        }
        catch (Exception e)
        {
            _logger.LogError($"GetUserById - {id} - Exception: " + e.Message);
            _logger.LogError($"GetUserById - {id} - Inner Exception: " + e.InnerException);
            return StatusCode(500);
        }
    }
    
    [Route("")]
    [HttpGet()]
    public async Task<ActionResult> GetAllUsers([FromServices] DaprClient daprClient)
    {
        try
        {
            _logger.LogInformation($"GetAllUsers");
            return Ok(await _persistenceService.RetrieveUsers());
        }
        catch (Exception e)
        {
            _logger.LogError($"GetAllUsers - Exception: " + e.Message);
            return StatusCode(500);
        }
    }

    [Route("donors")]
    [HttpGet()]
    public async Task<ActionResult> RetrieveDonorUsers([FromServices] DaprClient daprClient)
    {
        try
        {
            _logger.LogInformation($"RetrieveDonorUsers");
            return Ok(await _persistenceService.RetrieveDonorUsers());
        }
        catch (Exception e)
        {
            _logger.LogError($"RetrieveDonorUsers - Exception: " + e.Message);
            return StatusCode(500);
        }
    }

    [Route("validations/{identify}")]
    [HttpPost()]
    public async Task<ActionResult> ValidateUserVerificationById(string identify, [FromServices] DaprClient daprClient)
    {
        try
        {
            _logger.LogInformation($"ValidateUserVerificationById - {identify}");
            var actorId = new ActorId(identify);
            var proxy = ActorProxy.Create<IUserActor>(actorId, nameof(UserActor));
            bool verified = await proxy.IsVerified();

            if (verified)
            {
                return Ok();
            }
            else 
            {
                return StatusCode(401);
            }
        }
        catch (Exception e)
        {
            _logger.LogError($"ValidateUserVerificationById - {identify} - Exception: " + e.Message);
            _logger.LogError($"ValidateUserVerificationById - {identify} - Inner Exception: " + e.InnerException);
            return StatusCode(500);
        }
    }
    
    // Donor users are auto-created using the verification method
    // Organizer users are created using this API Endpoint so we can set their preferences including the 
    // institution they organize 
    [Route("")]
    [HttpPost()]
    public async Task<ActionResult> CreateUserAsync([FromBody]User user, [FromServices] DaprClient daprClient)
    {
        try
        {
            _logger.LogInformation($"CreateUserAsync - {user.Identifier}");
            await _persistenceService.PersistUser(user);
            return Ok();
        }
        catch (Exception e)
        {
            _logger.LogError($"CreateUserAsync - {user.Identifier} - Exception: " + e.Message);
            _logger.LogError($"CreateUserAsync - {user.Identifier} - Inner Exception: " + e.InnerException);
            return StatusCode(500);
        }
    }

    // Called by the frontend to generate a verification
    [Route("verifications/{identify}")]
    [HttpPost()]
    public async Task<ActionResult> InitiateVerificationAsync(string identify, [FromServices] DaprClient daprClient)
    {
        try
        {
            _logger.LogInformation($"InitiateVerificationAsync - {identify}");
            var actorId = new ActorId(identify);
            var proxy = ActorProxy.Create<IUserActor>(actorId, nameof(UserActor));
            await proxy.GenerateVerification();
            return Ok();
        }
        catch (Exception e)
        {
            _logger.LogError($"InitiateVerificationAsync - {identify} - Exception: " + e.Message);
            _logger.LogError($"InitiateVerificationAsync - {identify} - Inner Exception: " + e.InnerException);
            return StatusCode(500);
        }
    }
    
    // Called by the frontend to validate a verification code
    [Route("verifications/{identify}/{code}")]
    [HttpPost()]
    public async Task<ActionResult> ValidateVerificationAsync(string identify, string code, [FromServices] DaprClient daprClient)
    {
        try
        {
            _logger.LogInformation($"ValidateVerificationAsync - {identify}");
            var actorId = new ActorId(identify);
            var proxy = ActorProxy.Create<IUserActor>(actorId, nameof(UserActor));
            var response = await proxy.ValidateVerification(code);

            if (response.Verified)
            {
                return Ok(response);
            }
            else 
            {
                return StatusCode(401);
            }
        }
        catch (Exception e)
        {
            _logger.LogError($"ValidateVerificationAsync - {identify} - Exception: " + e.Message);
            _logger.LogError($"ValidateVerificationAsync - {identify} - Inner Exception: " + e.InnerException);
            return StatusCode(500);
        }
    }
}
