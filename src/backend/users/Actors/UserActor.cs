namespace pledgemanager.backend.users.Actors;

public class UserActor : Actor, IUserActor
{
    // ACTOR STATES
    private const string USER_STATE_NAME = "user_state";
    private const string VERIFICATIONS_STATE_NAME = "user_verifications_state";

    private const int VERIFICATION_ELAPSED_PERIOD_SECS = 180;

    private DaprClient _daprClient;
    private IEnvironmentService _envService;

    public UserActor(ActorHost host, DaprClient daprClient, IEnvironmentService envService) : base(host)
    {
        _daprClient = daprClient;
        _envService = envService;
    }

    protected override async Task OnActivateAsync()
    {
        Logger.LogInformation($"UserActor - OnActivateAsync [{this.Id.ToString()}] - Entry");
    }

    protected override async Task OnDeactivateAsync()
    {
        Logger.LogInformation($"UserActor - OnDeactivateAsync [{this.Id.ToString()}] - Entry");
    }

    public async Task Update(User usr)
    {
        //TODO:
    }

    public async Task<bool> IsVerified()
    {
        var response = false;
        Logger.LogInformation($"UserActor - IsVerified [{this.Id.ToString()}] - Entry");
        var user = await this.GetUserState();
        List<UserVerificationTransaction> xactions = await this.GetVerificationXactionsState();

        Logger.LogInformation($"UserActor - IsVerified [{this.Id.ToString()}]");
        var xaction = xactions.
            Take(1). // Limit to the last one only
            OrderByDescending(x => x.VerificationResponseTime).
            Where(x => x.VerificationAllowedTime > DateTime.Now && x.Verified == true). 
            FirstOrDefault();
        if (xaction != null)
        {
            response = true;
        }

        return response;
    }

    public async Task GenerateVerification() 
    {
        Logger.LogInformation($"UserActor - GenerateVerification [{this.Id.ToString()}] - Entry");
        var user = await this.GetUserState();
        if (user.SignupTime == null)
        {
            user.SignupTime = DateTime.Now;
        }

        List<UserVerificationTransaction> xactions = await this.GetVerificationXactionsState();

        Logger.LogInformation($"UserActor - GenerateVerification [{this.Id.ToString()}]");
        UserVerificationTransaction xaction = new UserVerificationTransaction();
        xaction.VerificationRequestTime = DateTime.Now;
        xaction.UserIdentifier = user.Identifier;
        xaction.VerificationMethod = user.VerificationMethod;
        xaction.Code = Constants.VERIFICATION_TEMP_CODE; //TODO:
        xactions.Add(xaction);

        //TODO: Depending on the verification method, we email or SMS

        user.Verifications = xactions.Where(d => d.VerificationRequestTime != null).OrderByDescending(d => d.VerificationRequestTime).Take(10).ToList(); 

        await this.SaveUserState(user);
        await this.SaveVerificationXactionsState(xactions);
        await this._daprClient.SaveStateAsync<User>(_envService.GetStateStoreName(), user.Identifier, user);
    }

    public async Task<UserVerificationResponse> ValidateVerification(string code) 
    {
        var response = new UserVerificationResponse();
        Logger.LogInformation($"UserActor - ValidateVerification [{this.Id.ToString()}] - Entry");
        var user = await this.GetUserState();
        List<UserVerificationTransaction> xactions = await this.GetVerificationXactionsState();

        Logger.LogInformation($"UserActor - ValidateVerification [{this.Id.ToString()}]");
        var xaction = xactions.
            Take(1). // Limit to the last one only
            OrderByDescending(x => x.VerificationResponseTime).
            Where(x => x.Code == code).
            FirstOrDefault();
        if (xaction != null)
        {
            xaction.VerificationResponseTime = DateTime.Now;
            var elapsedSeconds = (DateTime.Now - xaction.VerificationRequestTime).TotalSeconds;
            Logger.LogInformation($"UserActor - ValidateVerification [{this.Id.ToString()}] - elpased seconds: {elapsedSeconds}");
            if (elapsedSeconds <= VERIFICATION_ELAPSED_PERIOD_SECS)
            {
                xaction.Verified = true;
                xaction.VerificationAllowedTime = DateTime.Now.AddMinutes(Constants.USER_VALIDATION_PERIOD);
                user.LastVerificationTime = DateTime.Now;
            }
        }

        user.Verifications = xactions.Where(d => d.VerificationRequestTime != null).OrderByDescending(d => d.VerificationRequestTime).Take(10).ToList(); 

        await this.SaveUserState(user);
        await this.SaveVerificationXactionsState(xactions);
        await this._daprClient.SaveStateAsync<User>(_envService.GetStateStoreName(), user.Identifier, user);

        response.Verified = xaction != null ? xaction.Verified : false;
        response.Type = user.Type;
        response.InstitutionIdentifier = user.InstitutionIdentifier;
        return response;
    }

    private async Task<User> GetUserState() 
    {
        User user = new User();
        user.Identifier = this.Id.ToString();// WARNING: Make sure the user identifier match 
        var actorState = await this.StateManager.TryGetStateAsync<User>(USER_STATE_NAME);
        if (!actorState.HasValue) 
        {
            Logger.LogInformation($"UserActor - GetUserState [{this.Id.ToString()}]");
            var stateEntry = await _daprClient.GetStateEntryAsync<User>(_envService.GetStateStoreName(), this.Id.ToString());
            if (stateEntry != null && stateEntry.Value != null)
            {
                user = stateEntry.Value;
            }
            else 
            {
                user.Phone = this.Id.ToString();
            }

            await this.StateManager.SetStateAsync(USER_STATE_NAME, user);
            Logger.LogInformation($"UserActor - GetUserState [{this.Id.ToString()}]");
        }
        else 
        {
            user = actorState.Value;
        }

        return user;
    }

    private async Task SaveUserState(User user) 
    {
        await this.StateManager.SetStateAsync(USER_STATE_NAME, user);
    }

    private async Task<List<UserVerificationTransaction>> GetVerificationXactionsState() 
    {
        List<UserVerificationTransaction> xactions = new List<UserVerificationTransaction>();
        var actorState = await this.StateManager.TryGetStateAsync<List<UserVerificationTransaction>>(VERIFICATIONS_STATE_NAME);
        if (!actorState.HasValue) 
        {
            Logger.LogInformation($"UserActor - GetVerificationXactionsState [{this.Id.ToString()}]");
            await this.StateManager.SetStateAsync(VERIFICATIONS_STATE_NAME, xactions);
            Logger.LogInformation($"UserActor - GetVerificationXactionsState [{this.Id.ToString()}]");
        } 
        else 
        {
            xactions = actorState.Value;
        }

        return xactions;
    }
    
    private async Task SaveVerificationXactionsState(List<UserVerificationTransaction> verifications) 
    {
        await this.StateManager.SetStateAsync(VERIFICATIONS_STATE_NAME, verifications);
    }
}