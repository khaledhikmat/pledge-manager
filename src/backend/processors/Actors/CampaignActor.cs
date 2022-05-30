namespace pledgemanager.backend.processors.Actors;

public class CampaignActor : Actor, ICampaignActor, IRemindable
{
    // ACTOR STATES
    private const string CAMPAIGN_STATE_NAME = "campaign_state";
    private const string UPDATES_STATE_NAME = "campaign_updates_state";
    private const string COMMANDS_STATE_NAME = "campaign_commands_state";
    private const string PLEDGES_STATE_NAME = "campaign_pledges_state";
    private const string DONORS_STATE_NAME = "campaign_donors_state";

    // ACTOR COMMANDS (TODO: Make the equates available in the actor interface)
    private const string COMMAND_START = "start";
    private const string COMMAND_STOP = "stop";
    private const string COMMAND_EXPORT = "export";
    private const string COMMAND_REMIND = "remind";
    private const string COMMAND_FEATURE = "feature";
    private const string COMMAND_UNFEATURE = "unfeature";
    private const string COMMAND_ARCHIVE = "archive";

    // REMINDERS
    private const string UPDATE_EXG_RATE_REMINDER_NAME = "update-exchange-rate-reminder";
    private const int UPDATE_EXG_RATE_REMINDER_STARTUP = 30;
    private const int UPDATE_EXG_RATE_REMINDER_PERIODIC = 3600;
    private const string WATCH_ACTIVE_REMINDER_NAME = "watch-active-reminder";
    private const int WATCH_ACTIVE_REMINDER_STARTUP = 30;
    private const int WATCH_ACTIVE_REMINDER_PERIODIC = 60;

    // TIMERS
    private const string EXTERNALIZE_TIMER_NAME = "externalize-timer";
    private const int EXTERNALIZE_TIMER_STARTUP = 30;
    private const int EXTERNALIZE_TIMER_PERIODIC = 10;
 
    private DaprClient _daprClient;

    public CampaignActor(ActorHost host, DaprClient daprClient) : base(host)
    {
        _daprClient = daprClient;
    }

    protected override async Task OnActivateAsync()
    {
        Logger.LogInformation($"CampaignActor - OnActivateAsync [{this.Id.ToString()}] - Entry");

        //Set a timer to externalize actor state to an outside store
        await RegisterTimerAsync(
            EXTERNALIZE_TIMER_NAME,
            nameof(ExternalizationTimerCallback),
            null,
            TimeSpan.FromSeconds(EXTERNALIZE_TIMER_STARTUP),  // Externalizion startup in secs
            TimeSpan.FromSeconds(EXTERNALIZE_TIMER_PERIODIC)  // Externalizion period in secs
        );

        // Setup required reminders
        //TODO: Not sure if best place!!!
        await RegisterReminder(UPDATE_EXG_RATE_REMINDER_NAME, UPDATE_EXG_RATE_REMINDER_STARTUP, UPDATE_EXG_RATE_REMINDER_PERIODIC);
        await RegisterReminder(WATCH_ACTIVE_REMINDER_NAME, WATCH_ACTIVE_REMINDER_STARTUP, WATCH_ACTIVE_REMINDER_PERIODIC);
    }

    protected override async Task OnDeactivateAsync()
    {
        Logger.LogInformation($"CampaignActor - OnDeactivateAsync [{this.Id.ToString()}] - Entry");
        await UnregisterTimerAsync(EXTERNALIZE_TIMER_NAME);
        //WARNING: Do not unregister the reminders...otherwise the actor will never update itself
        // await UnregisterReminderAsync(UPDATE_EXG_RATE_REMINDER_NAME);
        // await UnregisterReminderAsync(WATCH_ACTIVE_REMINDER_NAME);
    }

    public async Task Update(Campaign update)
    {
        Logger.LogInformation($"CampaignActor - Update [{this.Id.ToString()}] - Entry");
        var campaign = await this.GetCampaignState();
        List<Campaign> updates = await this.GetUpdatesState();

        Logger.LogInformation($"CampaignActor - Update [{this.Id.ToString()}] - Processing update: {update.LastUpdatedTime}");
        campaign.LastUpdatedTime = DateTime.Now;
        campaign.Institution = update.Institution;
        campaign.InstitutionIdentifier = update.InstitutionIdentifier;
        campaign.Title = update.Title;
        campaign.Description = update.Description;
        campaign.ImageUrl = update.ImageUrl;

        if (campaign.Currency != update.Currency)
        {
            campaign.Currency = update.Currency;
            //TODO: Adjust the campaign fund per new currency
            //WARNING: Leave the pedges and donors in their own currency
            //WARNING: They are locked with their own exchange rate 
        }

        if (campaign.LastItemsCount != update.LastItemsCount)
        {
            campaign.LastItemsCount = update.LastItemsCount;                

            if (campaign.Commands.Count > 0)
            {
                List<CampaignCommand> campaignCommands = await GetCommandsState();
                campaign.Commands = campaignCommands.OrderByDescending(d => d.CommandTime).Take(campaign.LastItemsCount).ToList(); 
            }

            if (campaign.Pledges.Count > 0)
            {
                List<Pledge> campaignPledges = await GetPledgesState();
                campaign.Pledges = campaignPledges.OrderByDescending(d => d.PledgeTime).Take(campaign.LastItemsCount).ToList(); 
            }

            if (campaign.Donors.Count > 0)
            {
                List<Donor> campaignDonors = await GetDonorsState();
                campaign.Donors = campaignDonors.OrderByDescending(d => d.Amount).Take(campaign.LastItemsCount).ToList(); 
            }
        }

        updates.Add(update);
        campaign.Updates = updates.OrderByDescending(d => d.LastUpdatedTime).Take(campaign.LastItemsCount).ToList(); 

        await this.SaveCampaignState(campaign);
        await this.SaveUpdatesState(updates);
    }

    public async Task Pledge(Pledge pledge)
    {
        Logger.LogInformation($"CampaignActor - Pledge [{this.Id.ToString()}] - Entry");
        var campaign = await this.GetCampaignState();
        List<Pledge> pledges = await this.GetPledgesState();
        List<Donor> donors = await this.GetDonorsState();

        Logger.LogInformation($"CampaignActor - Pledge [{this.Id.ToString()}] - Processing pledge - username: {pledge.UserName} - currency: {pledge.Currency}  amount: {pledge.Amount}");

        if (campaign.IsActive) 
        {
            campaign.LastRefreshTime = DateTime.Now;
            campaign.Fund += pledge.Amount;
            pledge.Currency = campaign.Currency; 
            pledge.ExchangeRate = campaign.ExchangeRate; // Lock the exg rate
            pledge.PercentageOfTotalFund = campaign.Fund > 0 ? ((pledge.Amount / campaign.Fund) * 100) : 0;
        }
        else 
        {
            pledge.Confirmation = "Campaign is inactive! No fund is added.";
        }

        pledges.Add(pledge);
        campaign.Pledges = pledges.OrderByDescending(d => d.PledgeTime).Take(campaign.LastItemsCount).ToList(); 
        campaign.PledgesCount = pledges.Count();
        campaign.DonorsCount = pledges.Select(d => d.UserName).Distinct().Count();

        var donor = donors.Where(d => pledge.UserName == d.UserName).FirstOrDefault();
        if (donor == null)
        {
            donor = new Donor();
            donor.UserName = pledge.UserName;
            donor.Name = pledge.UserName;
            donors.Add(donor);
        }

        donor.CampaignIdentifier = campaign.Identifier;

        if (campaign.IsActive) 
        {
            donor.Amount += pledge.Amount;
            donor.Currency = campaign.Currency; 
            donor.ExchangeRate = campaign.ExchangeRate; // Lock the exg rate
            donor.PercentageOfTotalFund = campaign.Fund > 0 ? ((donor.Amount / campaign.Fund) * 100) : 0;
        }

        campaign.Donors = donors.OrderByDescending(d => d.Amount).Take(campaign.LastItemsCount).ToList(); 

        await this.SaveCampaignState(campaign);
        await this.SavePledgesState(pledges);
        await this.SaveDonorsState(donors);
    }

    public async Task Command(CampaignCommand command)
    {
        Logger.LogInformation($"CampaignActor - Command [{this.Id.ToString()}] - Entry");
        var campaign = await this.GetCampaignState();
        List<CampaignCommand> commands = await this.GetCommandsState();

        Logger.LogInformation($"CampaignActor - Command [{this.Id.ToString()}] - Processing command: {command.Command}");

        if (command.Command == COMMAND_START)
        {
            Logger.LogInformation($"CampaignActor - Start [{this.Id.ToString()}]");
            campaign.Start = DateTime.Now;
            campaign.IsActive = true;
        }
        else if (command.Command == COMMAND_STOP)
        {
            Logger.LogInformation($"CampaignActor - Stop [{this.Id.ToString()}]");
            campaign.Stop = DateTime.Now;
            campaign.IsActive = false;
        }
        else if (command.Command == COMMAND_EXPORT)
        {
            Logger.LogInformation($"CampaignActor - Export [{this.Id.ToString()}]");
            //TODO:
        }
        else if (command.Command == COMMAND_REMIND)
        {
            Logger.LogInformation($"CampaignActor - Remind [{this.Id.ToString()}]");
            //TODO:
        }
        else if (command.Command == COMMAND_FEATURE)
        {
            Logger.LogInformation($"CampaignActor - Feature [{this.Id.ToString()}]");
            campaign.IsFeatured = true;
        }
        else if (command.Command == COMMAND_UNFEATURE)
        {
            Logger.LogInformation($"CampaignActor - Unfeature [{this.Id.ToString()}]");
            campaign.IsFeatured = false;
        }
        else if (command.Command == COMMAND_ARCHIVE)
        {
            Logger.LogInformation($"CampaignActor - Archive [{this.Id.ToString()}]");
            //TODO:
        }
        else 
        {
            command.Confirmation = $"Command {command.Command} is not recognized!";
        }

        commands.Add(command);
        campaign.Commands = commands.OrderByDescending(d => d.CommandTime).Take(campaign.LastItemsCount).ToList(); 
        await this.SaveCampaignState(campaign);
        await this.SaveCommandsState(commands);
    } 

    public async Task ReceiveReminderAsync(string reminderName, byte[] state, TimeSpan dueTime, TimeSpan period)
    {
        Logger.LogInformation($"CampaignActor - ReceiveReminderAsync [{this.Id.ToString()}] - Entry");
        var campaign = await this.GetCampaignState();

        if (reminderName == UPDATE_EXG_RATE_REMINDER_NAME) 
        {
            Logger.LogInformation($"CampaignActor - ReceiveReminderAsync [{this.Id.ToString()}] - Processing {UPDATE_EXG_RATE_REMINDER_NAME} reminder");

            if (campaign.Currency == "USD")
            {
                campaign.ExchangeRate = 1;
            }
            else 
            {
                //TODO: Process update exchange rate
            }
        }
        else if (reminderName == WATCH_ACTIVE_REMINDER_NAME) 
        {
            Logger.LogInformation($"CampaignActor - ReceiveReminderAsync [{this.Id.ToString()}] - Processing {WATCH_ACTIVE_REMINDER_NAME} reminder");

            //Process watch active
            bool currentState = campaign.IsActive;
            bool desiredState = currentState;
            Logger.LogInformation($"CampaignActor - ReceiveReminderAsync [{this.Id.ToString()}] - Campaign Start: {campaign.Start} - Campaign stop: {campaign.Stop}");
            if (campaign.Start != null && 
                campaign.Stop != null && 
                campaign.Start <= DateTime.Now && 
                campaign.Stop > DateTime.Now)
            {
                desiredState = true;
            }
            else
            {
                desiredState= false;
            }

            campaign.IsActive = desiredState;

            if (currentState != desiredState)
            {
                await this.StateManager.SetStateAsync(CAMPAIGN_STATE_NAME, campaign);
            }
        }
    }

    public async Task ExternalizationTimerCallback(byte[] state)
    {
        Logger.LogInformation($"CampaignActor - ExternalizationTimerCallback [{this.Id.ToString()}] - Entry");
        // Downstream processes might need to perform additional functions:
        // 1. State store update
        // 2. gRPC server (written in Go perhaps)
        // 3. SignalR Service (although this should be done faster as in: SaveCampaignState)
        // 4. Redis Streams 
        // 5. Update Institution Actor which in turn update city, state and country actors
        // The idea therefore is to enqueue this to a pub/sub or event grid so the update
        // can be delivered to the different downstream processsors without impacting 
        // the actor's performance.

        // TODO: for now, let us do it synchronously

        // Save the state to a state store
        Logger.LogInformation($"CampaignActor - ExternalizationTimerCallback [{this.Id.ToString()}] - Save to state store");
        var campaign = await GetCampaignState();
        campaign.LastRefreshTime = DateTime.Now;
        await this._daprClient.SaveStateAsync<Campaign>(Constants.DAPR_STORE_NAME, campaign.Identifier, campaign);

        // Update the parent institution
        try
        {
            Logger.LogInformation($"CampaignActor - ExternalizationTimerCallback [{this.Id.ToString()}] - Updating institution actor");

            var actorId = new ActorId(campaign.InstitutionIdentifier);
            var proxy = ActorProxy.Create<IFundSinkActor>(actorId, nameof(InstitutionActor));

            //WARNING: Huh? await proxy.Fund(campaign);
            var input = new FundSink();
            input.Identifier = campaign.Identifier;
            input.Name = campaign.Identifier;
            input.Currency = campaign.Currency;
            input.ExchangeRate = campaign.ExchangeRate;
            input.ChildrenCount = campaign.ChildrenCount;
            input.PledgesCount = campaign.PledgesCount;
            input.DonorsCount = campaign.DonorsCount;
            input.Fund = campaign.Fund;

            await proxy.Fund(input);
        }
        catch (Exception e)
        {
            Logger.LogError($"CampaignActor - ExternalizationTimerCallback [{this.Id.ToString()}] - Updating institution actor caused a failure: {e.Message}");
        }
    }

    private async Task<Campaign> GetCampaignState() 
    {
        Campaign campaign = new Campaign();
        var actorState = await this.StateManager.TryGetStateAsync<Campaign>(CAMPAIGN_STATE_NAME);
        if (!actorState.HasValue) 
        {
            Logger.LogInformation($"CampaignActor - GetCampaignState [{this.Id.ToString()}]");
            var stateEntry = await _daprClient.GetStateEntryAsync<Campaign>(Constants.DAPR_STORE_NAME, this.Id.ToString());
            if (stateEntry != null && stateEntry.Value != null)
            {
                campaign = stateEntry.Value;
            }
            else 
            {
                campaign = new Campaign();
                campaign.Identifier = this.Id.ToString();
            }

            await this.StateManager.SetStateAsync(CAMPAIGN_STATE_NAME, campaign);
            Logger.LogInformation($"CampaignActor - GetCampaignState [{this.Id.ToString()}]");
        }
        else 
        {
            campaign = actorState.Value;
        }

        return campaign;
    }

    private async Task SaveCampaignState(Campaign campaign) 
    {
        await this.StateManager.SetStateAsync(CAMPAIGN_STATE_NAME, campaign);
        //TODO: Externalize campaign to SignalR to allow for real-time updates
        Logger.LogInformation($"CampaignActor - Externalize campaign [{this.Id.ToString()}] to SignalR for real-time updates....");
    }

    private async Task<List<Pledge>> GetPledgesState() 
    {
        List<Pledge> pledges = new List<Pledge>();
        var actorState = await this.StateManager.TryGetStateAsync<List<Pledge>>(PLEDGES_STATE_NAME);
        if (!actorState.HasValue) 
        {
            Logger.LogInformation($"CampaignActor - GetPledgesState [{this.Id.ToString()}]");
            await this.StateManager.SetStateAsync(PLEDGES_STATE_NAME, pledges);
            Logger.LogInformation($"CampaignActor - GetPledgesState [{this.Id.ToString()}]");
        } 
        else 
        {
            pledges = actorState.Value;
        }

        return pledges;
    }
    
    private async Task SavePledgesState(List<Pledge> pledges) 
    {
        await this.StateManager.SetStateAsync(PLEDGES_STATE_NAME, pledges);
    }

    private async Task<List<Donor>> GetDonorsState() 
    {
        List<Donor> donors = new List<Donor>();
        var actorState = await this.StateManager.TryGetStateAsync<List<Donor>>(DONORS_STATE_NAME);
        if (!actorState.HasValue) 
        {
            Logger.LogInformation($"CampaignActor - GetDonorsState [{this.Id.ToString()}]");
            await this.StateManager.SetStateAsync(DONORS_STATE_NAME, donors);
            Logger.LogInformation($"CampaignActor - GetDonorsState [{this.Id.ToString()}]");
        } 
        else 
        {
            donors = actorState.Value;
        }

        return donors;
    }
    
    private async Task SaveDonorsState(List<Donor> donors) 
    {
        await this.StateManager.SetStateAsync(DONORS_STATE_NAME, donors);
    }

    private async Task<List<CampaignCommand>> GetCommandsState() 
    {
        List<CampaignCommand> commands = new List<CampaignCommand>();
        var actorState = await this.StateManager.TryGetStateAsync<List<CampaignCommand>>(COMMANDS_STATE_NAME);
        if (!actorState.HasValue) 
        {
            Logger.LogInformation($"CampaignActor - GetCommandsState [{this.Id.ToString()}]");
            await this.StateManager.SetStateAsync(COMMANDS_STATE_NAME, commands);
            Logger.LogInformation($"CampaignActor - GetCommandsState [{this.Id.ToString()}]");
        } 
        else 
        {
            commands = actorState.Value;
        }

        return commands;
    }
    
    private async Task SaveCommandsState(List<CampaignCommand> commands) 
    {
        await this.StateManager.SetStateAsync(COMMANDS_STATE_NAME, commands);
    }

    private async Task<List<Campaign>> GetUpdatesState() 
    {
        List<Campaign> updates = new List<Campaign>();
        var actorState = await this.StateManager.TryGetStateAsync<List<Campaign>>(UPDATES_STATE_NAME);
        if (!actorState.HasValue) 
        {
            Logger.LogInformation($"CampaignActor - GetUpdatesState [{this.Id.ToString()}]");
            await this.StateManager.SetStateAsync(UPDATES_STATE_NAME, updates);
            Logger.LogInformation($"CampaignActor - GetUpdatesState [{this.Id.ToString()}]");
        } 
        else 
        {
            updates = actorState.Value;
        }

        return updates;
    }
    
    private async Task SaveUpdatesState(List<Campaign> updates) 
    {
        await this.StateManager.SetStateAsync(UPDATES_STATE_NAME, updates);
    }

    private async Task RegisterReminder(string reminderName, int startUpSecs, int periodicSecs) 
    {
        await RegisterReminderAsync(
            reminderName, 
            null, 
            TimeSpan.FromSeconds(startUpSecs),   // Start after startupSecs the actor is activated 
            TimeSpan.FromSeconds(periodicSecs)   // Remind every periodSecs after that
        );    
    }
}