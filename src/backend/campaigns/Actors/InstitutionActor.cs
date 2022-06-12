namespace pledgemanager.backend.campaigns.Actors;

public class InstitutionActor : Actor, IFundSinkActor
{
    // ACTOR STATES
    private const string INSTITUTION_STATE_NAME = "institution_state";
    private const string FUNDS_STATE_NAME = "institution_funds_state";

    // TIMERS
    private const string EXTERNALIZE_TIMER_NAME = "externalize-timer";
    private const int EXTERNALIZE_TIMER_STARTUP = 30;
    private const int EXTERNALIZE_TIMER_PERIODIC = 10;

    private DaprClient _daprClient;

    public InstitutionActor(ActorHost host, DaprClient daprClient) : base(host)
    {
        _daprClient = daprClient;
    }

    protected override async Task OnActivateAsync()
    {
        Logger.LogInformation($"InstitutionActor - OnActivateAsync [{this.Id.ToString()}] - Entry");

        //Set a timer to externalize actor state to an outside store
        await RegisterTimerAsync(
            EXTERNALIZE_TIMER_NAME,
            nameof(ExternalizationTimerCallback),
            null,
            TimeSpan.FromSeconds(EXTERNALIZE_TIMER_STARTUP),  // Externalizion startup in secs
            TimeSpan.FromSeconds(EXTERNALIZE_TIMER_PERIODIC)  // Externalizion period in secs
        );
    }

    protected override async Task OnDeactivateAsync()
    {
        Logger.LogInformation($"InstitutionActor - OnDeactivateAsync [{this.Id.ToString()}] - Entry");
        await UnregisterTimerAsync(EXTERNALIZE_TIMER_NAME);
    }

    public async Task Fund(FundSink fund)
    {
        Logger.LogInformation($"InstitutionActor - Update [{this.Id.ToString()}] - Entry");
        var institution = await this.GetInstitutionState();
        var funds = await this.GetFundsState();

        int childrenCount = 0;
        int pledgesCount = 0;
        int donorsCount = 0;
        double amount = 0;

        funds[fund.Identifier] = fund;

        foreach(KeyValuePair<string, FundSink> entry in funds)
        {
            childrenCount++;
            pledgesCount += entry.Value.FulfilledPledgesCount;
            donorsCount += entry.Value.DonorsCount;
            amount += entry.Value.Fund;
        }

        institution.LastRefreshTime = DateTime.Now;
        institution.ChildrenCount = childrenCount;
        institution.FulfilledPledgesCount = pledgesCount;
        institution.DonorsCount = donorsCount;
        institution.Fund = amount;

        await this.SaveInstitutionState(institution);
        await this.SaveFundsState(funds);
    }


    public async Task ExternalizationTimerCallback(byte[] state)
    {
        Logger.LogInformation($"InstitutionActor - ExternalizationTimerCallback [{this.Id.ToString()}] - Entry");
        // TODO: for now, let us do it synchronously

        // Save the state to a state store
        Logger.LogInformation($"InstitutionActor - ExternalizationTimerCallback [{this.Id.ToString()}] - Save to state store");
        var institution = await GetInstitutionState();
        await this._daprClient.SaveStateAsync<Institution>(Constants.DAPR_CAMPAIGNS_STORE_NAME, institution.Identifier, institution);

        //WARNING: Huh? await proxy.Fund(institution);
        var input = new FundSink();
        input.Identifier = institution.City;
        input.Name = institution.City;
        input.Type = FundSinkTypes.City;
        input.Currency = institution.Currency;
        input.ExchangeRate = institution.ExchangeRate;
        input.ChildrenCount = institution.ChildrenCount;
        input.FulfilledPledgesCount = institution.FulfilledPledgesCount;
        input.DonorsCount = institution.DonorsCount;
        input.Fund = institution.Fund;

        // Update the parent city
        try
        {
            Logger.LogInformation($"InstitutionActor - ExternalizationTimerCallback [{this.Id.ToString()}] - Updating city actor");

            var actorId = new ActorId(institution.City);
            var proxy = ActorProxy.Create<IFundSinkActor>(actorId, nameof(RegionalActor));
            await proxy.Fund(input);
        }
        catch (Exception e)
        {
            Logger.LogError($"InstitutionActor - ExternalizationTimerCallback [{this.Id.ToString()}] - Updating city actor caused a failure: {e.Message}");
        }

        // Update the parent state
        input.Identifier = institution.State;
        input.Name = institution.State;
        input.Type = FundSinkTypes.State;
        try
        {
            Logger.LogInformation($"InstitutionActor - ExternalizationTimerCallback [{this.Id.ToString()}] - Updating state actor");

            var actorId = new ActorId(institution.State);
            var proxy = ActorProxy.Create<IFundSinkActor>(actorId, nameof(RegionalActor));
            await proxy.Fund(input);
        }
        catch (Exception e)
        {
            Logger.LogError($"InstitutionActor - ExternalizationTimerCallback [{this.Id.ToString()}] - Updating state actor caused a failure: {e.Message}");
        }

        // Update the parent country
        input.Identifier = institution.Country;
        input.Name = institution.Country;
        input.Type = FundSinkTypes.Country;
        try
        {
            Logger.LogInformation($"InstitutionActor - ExternalizationTimerCallback [{this.Id.ToString()}] - Updating country actor");

            var actorId = new ActorId(institution.Country);
            var proxy = ActorProxy.Create<IFundSinkActor>(actorId, nameof(RegionalActor));
            await proxy.Fund(input);
        }
        catch (Exception e)
        {
            Logger.LogError($"InstitutionActor - ExternalizationTimerCallback [{this.Id.ToString()}] - Updating country actor caused a failure: {e.Message}");
        }

        // Update the parent global
        input.Identifier = "Global";
        input.Name = "Global";
        input.Type = FundSinkTypes.Global;
        try
        {
            Logger.LogInformation($"InstitutionActor - ExternalizationTimerCallback [{this.Id.ToString()}] - Updating global actor");

            var actorId = new ActorId("Global");
            var proxy = ActorProxy.Create<IFundSinkActor>(actorId, nameof(RegionalActor));
            await proxy.Fund(input);
        }
        catch (Exception e)
        {
            Logger.LogError($"InstitutionActor - ExternalizationTimerCallback [{this.Id.ToString()}] - Updating global actor caused a failure: {e.Message}");
        }
    }

    private async Task<Institution> GetInstitutionState() 
    {
        Institution institution = new Institution();
        var actorState = await this.StateManager.TryGetStateAsync<Institution>(INSTITUTION_STATE_NAME);
        if (!actorState.HasValue) 
        {
            Logger.LogInformation($"InstitutionActor - GetInstitutionState [{this.Id.ToString()}]");
            var stateEntry = await _daprClient.GetStateEntryAsync<Institution>(Constants.DAPR_CAMPAIGNS_STORE_NAME, this.Id.ToString());
            if (stateEntry != null && stateEntry.Value != null)
            {
                institution = stateEntry.Value;
            }
            else 
            {
                institution = new Institution();
                institution.Identifier = this.Id.ToString();
            }

            await this.StateManager.SetStateAsync(INSTITUTION_STATE_NAME, institution);
            Logger.LogInformation($"InstitutionActor - GetInstitutionState [{this.Id.ToString()}]");
        }
        else 
        {
            institution = actorState.Value;
        }

        return institution;
    }

    private async Task SaveInstitutionState(Institution institution) 
    {
        await this.StateManager.SetStateAsync(INSTITUTION_STATE_NAME, institution);
    }

    private async Task<Dictionary<string, FundSink>> GetFundsState() 
    {
        Dictionary<string, FundSink> funds = new Dictionary<string, FundSink>();
        var actorState = await this.StateManager.TryGetStateAsync<Dictionary<string, FundSink>>(FUNDS_STATE_NAME);
        if (!actorState.HasValue) 
        {
            Logger.LogInformation($"InstitutionActor - GetFundsState [{this.Id.ToString()}]");
            await this.StateManager.SetStateAsync(FUNDS_STATE_NAME, funds);
            Logger.LogInformation($"InstitutionActor - GetFundsState [{this.Id.ToString()}]");
        } 
        else 
        {
            funds = actorState.Value;
        }

        return funds;
    }
    
    private async Task SaveFundsState(Dictionary<string, FundSink> funds) 
    {
        await this.StateManager.SetStateAsync(FUNDS_STATE_NAME, funds);
    }
}
