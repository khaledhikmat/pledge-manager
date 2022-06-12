namespace pledgemanager.backend.campaigns.Actors;

public class RegionalActor : Actor, IFundSinkActor
{
    // ACTOR STATES
    private const string REGION_STATE_NAME = "region_state";
    private const string FUNDS_STATE_NAME = "region_funds_state";

    // TIMERS
    private const string EXTERNALIZE_TIMER_NAME = "externalize-timer";
    private const int EXTERNALIZE_TIMER_STARTUP = 30;
    private const int EXTERNALIZE_TIMER_PERIODIC = 10;

    private DaprClient _daprClient;

    public RegionalActor(ActorHost host, DaprClient daprClient) : base(host)
    {
        _daprClient = daprClient;
    }

    protected override async Task OnActivateAsync()
    {
        Logger.LogInformation($"RegionalActor - OnActivateAsync [{this.Id.ToString()}] - Entry");

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
        Logger.LogInformation($"RegionalActor - OnDeactivateAsync [{this.Id.ToString()}] - Entry");
        await UnregisterTimerAsync(EXTERNALIZE_TIMER_NAME);
    }

    public async Task Fund(FundSink fund)
    {
        Logger.LogInformation($"RegionalActor - Update [{this.Id.ToString()}] - Entry");
        var region = await this.GetRegionState();
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

        region.LastRefreshTime = DateTime.Now;
        region.ChildrenCount = childrenCount;
        region.FulfilledPledgesCount = pledgesCount;
        region.DonorsCount = donorsCount;
        region.Fund = amount;

        await this.SaveRegionState(region);
        await this.SaveFundsState(funds);
    }


    public async Task ExternalizationTimerCallback(byte[] state)
    {
        Logger.LogInformation($"RegionalActor - ExternalizationTimerCallback [{this.Id.ToString()}] - Entry");
        // TODO: for now, let us do it synchronously

        // Save the state to a state store
        Logger.LogInformation($"RegionalActor - ExternalizationTimerCallback [{this.Id.ToString()}] - Save to state store");
        var region = await GetRegionState();
        await this._daprClient.SaveStateAsync<FundSink>(Constants.DAPR_CAMPAIGNS_STORE_NAME, region.Identifier, region);
    }

    private async Task<FundSink> GetRegionState() 
    {
        FundSink region = new FundSink();
        var actorState = await this.StateManager.TryGetStateAsync<FundSink>(REGION_STATE_NAME);
        if (!actorState.HasValue) 
        {
            Logger.LogInformation($"RegionalActor - GetRegionState [{this.Id.ToString()}]");
            var stateEntry = await _daprClient.GetStateEntryAsync<FundSink>(Constants.DAPR_CAMPAIGNS_STORE_NAME, this.Id.ToString());
            if (stateEntry != null && stateEntry.Value != null)
            {
                region = stateEntry.Value;
            }
            else 
            {
                region = new FundSink();
                region.Identifier = this.Id.ToString();
                region.Name = this.Id.ToString();
            }

            await this.StateManager.SetStateAsync(REGION_STATE_NAME, region);
            Logger.LogInformation($"RegionalActor - GetRegionState [{this.Id.ToString()}]");
        }
        else 
        {
            region = actorState.Value;
        }

        return region;
    }

    private async Task SaveRegionState(FundSink region) 
    {
        await this.StateManager.SetStateAsync(REGION_STATE_NAME, region);
    }

    private async Task<Dictionary<string, FundSink>> GetFundsState() 
    {
        Dictionary<string, FundSink> funds = new Dictionary<string, FundSink>();
        var actorState = await this.StateManager.TryGetStateAsync<Dictionary<string, FundSink>>(FUNDS_STATE_NAME);
        if (!actorState.HasValue) 
        {
            Logger.LogInformation($"RegionalActor - GetFundsState [{this.Id.ToString()}]");
            await this.StateManager.SetStateAsync(FUNDS_STATE_NAME, funds);
            Logger.LogInformation($"RegionalActor - GetFundsState [{this.Id.ToString()}]");
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
