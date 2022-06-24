namespace pledgemanager.backend.campaigns.Actors;

public class CampaignActor : Actor, ICampaignActor, IRemindable
{
    // ACTOR STATES
    private const string CAMPAIGN_STATE_NAME = "campaign_state";
    private const string UPDATES_STATE_NAME = "campaign_updates_state";
    private const string COMMANDS_STATE_NAME = "campaign_commands_state";
    private const string PLEDGES_STATE_NAME = "campaign_pledges_state";
    private const string DONORS_STATE_NAME = "campaign_donors_state";

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
    private SignalRRestService _signalRService;

    public CampaignActor(ActorHost host, DaprClient daprClient, SignalRRestService signalRService) : base(host)
    {
        _daprClient = daprClient;
        _signalRService = signalRService; 
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

    public async Task<List<FundSinkPeriod>> GetPeriods()
    {
        List<FundSinkPeriod> periods = new();
        Logger.LogInformation($"CampaignActor - GetPeriods [{this.Id.ToString()}] - Entry");
        Campaign campaign = await this.GetCampaignState();
        List<Pledge> pledges = await this.GetPledgesState();
        string error = "";

        try
        {
            periods = GetPledgePeriods(campaign, pledges);
        } 
        catch(Exception e)
        {
            error = e.Message;
            Logger.LogError($"CampaignActor - GetPeriods - error: {e.Message}");
        } 
        finally
        {
        }

        return periods;
    }

    public async Task<string> Update(Campaign update)
    {
        Logger.LogInformation($"CampaignActor - Update [{this.Id.ToString()}] - Entry");
        var campaign = await this.GetCampaignState();
        List<Campaign> updates = await this.GetUpdatesState();
        string error = "";

        try
        {
            if (update == null)
            {
                throw new Exception("NULL Update!");
            }

            Logger.LogInformation($"CampaignActor - Update [{this.Id.ToString()}] - Processing update: {update.LastUpdatedTime}");
            campaign.LastUpdatedTime = DateTime.Now;
            campaign.Institution = update.Institution;
            campaign.InstitutionIdentifier = update.InstitutionIdentifier;
            campaign.Title = update.Title;
            campaign.Description = update.Description;
            campaign.ImageUrl = update.ImageUrl;
            campaign.IsFeatured = update.IsFeatured;
            campaign.LastItemsCount = update.LastItemsCount;
            campaign.Goal = update.Goal;
            campaign.PeriodsCount = update.PeriodsCount;
            campaign.PeriodType = update.PeriodType;

            campaign.Behavior = update.Behavior;

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

                if (campaign.FulfilledPledges.Count > 0)
                {
                    List<Pledge> campaignPledges = await GetPledgesState();
                    campaign.FulfilledPledges = campaignPledges.OrderByDescending(d => d.PledgeTime).Take(campaign.LastItemsCount).ToList(); 
                    campaign.PendingApprovalPledges = campaignPledges.Where(d => d.ApprovedTime == null && d.RejectedTime == null).OrderByDescending(d => d.PledgeTime).Take(campaign.LastItemsCount).ToList(); 
                    campaign.ErroredPledges = campaignPledges.Where(d => !string.IsNullOrEmpty(d.Error)).OrderByDescending(d => d.PledgeTime).Take(campaign.LastItemsCount).ToList(); 
                }

                if (campaign.Donors.Count > 0)
                {
                    List<Donor> campaignDonors = await GetDonorsState();
                    campaign.Donors = campaignDonors.OrderByDescending(d => d.Amount).Take(campaign.LastItemsCount).ToList(); 
                }
            }

            updates.Add(update);
        } 
        catch(Exception e)
        {
            error = e.Message;
        } 
        finally
        {
            await this.SaveCampaignState(campaign);
            await this.SaveUpdatesState(updates);
            await _daprClient.SaveStateAsync<Campaign>(Constants.DAPR_CAMPAIGNS_STORE_NAME, campaign.Identifier, campaign);
        }

        return error;
    }

    public async Task Pledge(Pledge pledge)
    {
        Logger.LogInformation($"CampaignActor - Pledge against campaign [{this.Id.ToString()}] - Entry");
        var campaign = await this.GetCampaignState();
        List<Pledge> pledges = await this.GetPledgesState();
        List<Donor> donors = await this.GetDonorsState();
        Donor donor = null;

        try
        {
            if (pledge == null)
            {
                throw new Exception("Pledge is null!");
            }

            if (!campaign.IsActive) 
            {
                throw new Exception($"Campaign [{this.Id.ToString()}] is not active!!!");
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

            if (campaign.IsPledgeApproved(pledge))
            {
                pledge.ApprovedTime = DateTime.Now;
                if (!pledge.IsMatch && !pledge.IsDeferred)
                {
                    pledge.FulfilledTime = DateTime.Now; // So we don't leave ligering...this is meant for conditional pledges
                }
            }

            pledge.Currency = campaign.Currency; 
            pledge.ExchangeRate = campaign.ExchangeRate; // Lock the exg rate
            pledge.PercentageOfTotalFund = campaign.Fund > 0 ? (pledge.Amount / campaign.Fund) : 0;

            pledges.Add(pledge);
            
            donor = donors.Where(d => pledge.UserName == d.UserName).FirstOrDefault();
            if (donor == null)
            {
                donor = new Donor();
                donor.UserName = pledge.UserName;
                donor.Name = pledge.UserName;
                donors.Add(donor);
            }

            donor.CampaignIdentifier = campaign.Identifier;
            donor.PledgesCount = pledges.Where(d => d.UserName == donor.UserName).Count();             
            donor.Amount += pledge.ApprovedTime != null ? pledge.Amount : 0;
            donor.Currency = campaign.Currency; 
            donor.ExchangeRate = campaign.ExchangeRate; // Lock the exg rate
            donor.PercentageOfTotalFund = campaign.Fund > 0 ? (donor.Amount / campaign.Fund) : 0;
        }
        catch (Exception e)
        {
            //TODO: How do we handle this? Pledge failed after we provided confirmation
            Logger.LogError($"CampaignActor - Processing pledge - error: {e.Message}");
            pledge.Error = $"Pledge process error: {e.Message}";
        }
        finally 
        {
            await this.RunPostPledgeProcessors(campaign, pledge, donor, pledges, donors);
        }
    }

    public async Task Command(CampaignCommand command)
    {
        Logger.LogInformation($"CampaignActor - Command [{this.Id.ToString()}] - Entry");
        var campaign = await this.GetCampaignState();
        List<Pledge> pledges = await this.GetPledgesState();
        List<Donor> donors = await this.GetDonorsState();
        List<CampaignCommand> commands = await this.GetCommandsState();

        try 
        {
            Logger.LogInformation($"CampaignActor - Command [{this.Id.ToString()}] - Processing command: {command.Command}");

            if (command.Command == CampaignCommand.START)
            {
                Logger.LogInformation($"CampaignActor - Start [{this.Id.ToString()}]");
                campaign.Start = DateTime.Now;
                campaign.IsActive = true;
            }
            else if (command.Command == CampaignCommand.STOP)
            {
                Logger.LogInformation($"CampaignActor - Stop [{this.Id.ToString()}]");
                campaign.Stop = DateTime.Now;
                campaign.IsActive = false;
                await ProcessDeferredPledges();
            }
            else if (command.Command == CampaignCommand.EXPORT)
            {
                Logger.LogInformation($"CampaignActor - Export [{this.Id.ToString()}]");
                //TODO:
            }
            else if (command.Command == CampaignCommand.REMIND)
            {
                Logger.LogInformation($"CampaignActor - Remind [{this.Id.ToString()}]");
                //TODO:
            }
            else if (command.Command == CampaignCommand.FEATURE)
            {
                Logger.LogInformation($"CampaignActor - Feature [{this.Id.ToString()}]");
                campaign.IsFeatured = true;
            }
            else if (command.Command == CampaignCommand.UNFEATURE)
            {
                Logger.LogInformation($"CampaignActor - Unfeature [{this.Id.ToString()}]");
                campaign.IsFeatured = false;
            }
            else if (command.Command == CampaignCommand.ARCHIVE)
            {
                Logger.LogInformation($"CampaignActor - Archive [{this.Id.ToString()}]");
                //TODO:
            }
            else if (command.Command == CampaignCommand.APPROVE_PLEDGE)
            {
                Logger.LogInformation($"CampaignActor - approve pledge [{this.Id.ToString()}]");
                var pledge = pledges.Where(p => p.Identifier == command.Arg1).FirstOrDefault();
                if (pledge != null && 
                    pledge.ApprovedTime == null && 
                    pledge.RejectedTime == null)
                {
                    pledge.ApprovedTime = DateTime.Now;
                    if (!pledge.IsMatch && !pledge.IsDeferred)
                    {
                        pledge.FulfilledTime = DateTime.Now; // So we don't leave lingering...this is meant for conditional pledges
                    }

                    await this.RunPostPledgeProcessors(campaign, pledge, null, pledges, donors);
                }
                else
                {
                    throw new Exception($"Pledge [{this.Id.ToString()}] is already approved or rejected!");
                }
            }
            else if (command.Command == CampaignCommand.REJECT_PLEDGE)
            {
                Logger.LogInformation($"CampaignActor - reject pledge [{this.Id.ToString()}]");
                var pledge = pledges.Where(p => p.Identifier == command.Arg1).FirstOrDefault();
                if (pledge != null && 
                    pledge.ApprovedTime == null && 
                    pledge.RejectedTime == null)
                {
                    pledge.RejectedTime = DateTime.Now;

                    await this.RunPostPledgeProcessors(campaign, pledge, null, pledges, donors);
                }
                else
                {
                    throw new Exception($"Pledge [{this.Id.ToString()}] is already approved or rejected!");
                }
            }
            else 
            {
                throw new Exception($"Command {command.Command} is not recognized!");
            }

            commands.Add(command);
        }
        catch (Exception e)
        {
            Logger.LogInformation($"Command {command.Command} - Error: {e.Message}");
            command.Error = e.Message;
        }
        finally
        {
            await this.SaveCampaignState(campaign);
            await this.SaveCommandsState(commands);
        }
    } 

    public async Task ReceiveReminderAsync(string reminderName, byte[] state, TimeSpan dueTime, TimeSpan period)
    {
        Logger.LogInformation($"CampaignActor - ReceiveReminderAsync [{this.Id.ToString()}] - Entry");
        var campaign = await this.GetCampaignState();

        try
        {
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

                    if (campaign.IsActive && currentState != desiredState)
                    {
                        await ProcessDeferredPledges();
                    }
                }

            }
            else
            {
                throw new Exception($"Reminder {reminderName} is not supported!");
            }
        } 
        catch(Exception e)
        {
            Logger.LogError($"CampaignActor - ReceiveReminderAsync [{this.Id.ToString()}] - Error: {e.Message}");
        } 
    }

    private async Task ExternalizationTimerCallback(byte[] state)
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
        await this._daprClient.SaveStateAsync<Campaign>(Constants.DAPR_CAMPAIGNS_STORE_NAME, campaign.Identifier, campaign);

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
            input.FulfilledPledgesCount = campaign.FulfilledPledgesCount;
            input.DonorsCount = campaign.DonorsCount;
            input.Fund = campaign.Fund;

            await proxy.Fund(input);
        }
        catch (Exception e)
        {
            Logger.LogError($"CampaignActor - ExternalizationTimerCallback [{this.Id.ToString()}] - Updating institution actor caused a failure: {e.Message}");
        }
    }

    private async Task ProcessDeferredPledges()
    {
        var campaign = await this.GetCampaignState();
        List<Pledge> pledges = await this.GetPledgesState();
        List<Donor> donors = await this.GetDonorsState();

        //WARNING: Sort by smallest to maximize the chance of processing all deferred pledges
        var deferredPledges = pledges.Where(d => d.IsDeferred && d.ApprovedTime != null && d.FulfilledTime == null).OrderBy(d => d.Amount).ToList();
        foreach (Pledge deferredPledge in deferredPledges)
        {
            string deferredPledgeId = deferredPledge.Identifier;

            //WARNING: Re-retrieve
            campaign = await this.GetCampaignState();
            pledges = await this.GetPledgesState();
            donors = await this.GetDonorsState();

            var actualDeferredPledge = pledges.Where(p => p.Identifier == deferredPledgeId).FirstOrDefault();
            if (actualDeferredPledge != null && 
                campaign.Fund >= actualDeferredPledge.Amount)
            {
                actualDeferredPledge.FulfilledTime = DateTime.Now;
                await this.RunPostPledgeProcessors(campaign, actualDeferredPledge, null, pledges, donors);
            }
        }
    }

    private async Task RunPostPledgeProcessors(
        Campaign campaign, 
        Pledge pledge, 
        Donor donor, 
        List<Pledge> pledges, 
        List<Donor> donors)
    {
        try
        {
            Logger.LogInformation($"CampaignActor - RunPostPledgeProcessors [{this.Id.ToString()}] - Entry");
            
            campaign.LastRefreshTime = DateTime.Now;

            if (pledge != null)
            {
                campaign.Fund += pledge.FulfilledTime != null ? pledge.Amount : 0;
            }

            Logger.LogInformation($"CampaignActor - RunPostPledgeProcessors [{this.Id.ToString()}] - Processing conditional pledges...");
            var matchPledge = pledges.Where(m => m.ApprovedTime != null && m.RejectedTime == null && m.FulfilledTime == null && m.IsMatch).OrderBy(m => m.ApprovedTime).Take(1).FirstOrDefault();

            if (matchPledge != null)
            {
                campaign.MatchFund += pledge != null && pledge.FulfilledTime != null ? pledge.Amount : 0;
            }

            if (matchPledge != null && campaign.MatchFund >= matchPledge.Amount)
            {
                matchPledge.FulfilledTime = DateTime.Now;
                campaign.Fund += matchPledge.Amount;
                campaign.MatchFund = 0;

                await this._daprClient.SaveStateAsync<Pledge>(Constants.DAPR_CAMPAIGNS_STORE_NAME, matchPledge.Identifier, matchPledge);
            }

            pledges = pledges.Select(p => {p.PercentageOfTotalFund = campaign.Fund > 0 ? p.Amount / campaign.Fund : 0; return p;}).ToList();
            donors = donors.Select(d => {d.PercentageOfTotalFund = campaign.Fund > 0 ? d.Amount / campaign.Fund : 0; return d;}).ToList();

            Logger.LogInformation($"CampaignActor - RunPostPledgeProcessors [{this.Id.ToString()}] - Processing lists...");
            campaign.FulfilledPledges = pledges.Where(d => d.FulfilledTime != null).OrderByDescending(d => d.PledgeTime).Take(campaign.LastItemsCount).ToList(); 
            campaign.FulfilledPledgesCount = pledges.Where(d => d.FulfilledTime != null).Count();
            campaign.ActiveMatchPledge = pledges.Where(d => d.IsMatch && d.ApprovedTime != null && d.FulfilledTime == null).OrderBy(d => d.ApprovedTime).Take(1).FirstOrDefault(); 
            campaign.PendingMatchPledges = pledges.Where(d => d.IsMatch && d.ApprovedTime != null && d.FulfilledTime == null && (campaign.ActiveMatchPledge != null ? d.Identifier != campaign.ActiveMatchPledge.Identifier : 0 == 0)).OrderBy(d => d.ApprovedTime).Take(campaign.LastItemsCount).ToList();
            campaign.PendingMatchPledgesCount = pledges.Where(d => d.IsMatch && d.ApprovedTime != null && d.FulfilledTime == null && (campaign.ActiveMatchPledge != null ? d.Identifier != campaign.ActiveMatchPledge.Identifier : 0 == 0)).Count();
            campaign.PendingMatchPledgesAmount = pledges.Where(d => d.IsMatch && d.ApprovedTime != null && d.FulfilledTime == null && (campaign.ActiveMatchPledge != null ? d.Identifier != campaign.ActiveMatchPledge.Identifier : 0 == 0)).Sum(p => p.Amount);
            campaign.PendingDeferredPledges = pledges.Where(d => d.IsDeferred && d.ApprovedTime != null && d.FulfilledTime == null).OrderBy(d => d.ApprovedTime).Take(campaign.LastItemsCount).ToList();
            campaign.PendingDeferredPledgesCount = pledges.Where(d => d.IsDeferred && d.ApprovedTime != null && d.FulfilledTime == null).Count();
            campaign.PendingDeferredPledgesAmount = pledges.Where(d => d.IsDeferred && d.ApprovedTime != null && d.FulfilledTime == null).Sum(p => p.Amount);
            campaign.PendingApprovalPledges = pledges.Where(d => d.ApprovedTime == null && d.RejectedTime == null).OrderBy(d => d.PledgeTime).Take(campaign.LastItemsCount).ToList(); 
            campaign.PendingApprovalPledgesCount = pledges.Where(d => d.ApprovedTime == null && d.RejectedTime == null).Count(); 
            campaign.RejectedPledges = pledges.Where(d => d.ApprovedTime == null && d.RejectedTime != null).OrderByDescending(d => d.PledgeTime).Take(campaign.LastItemsCount).ToList(); 
            campaign.RejectedPledgesCount = campaign.RejectedPledges.Count;             
            campaign.ErroredPledges = pledges.Where(d => !string.IsNullOrEmpty(d.Error)).OrderByDescending(d => d.PledgeTime).Take(campaign.LastItemsCount).ToList(); 
            campaign.ErroredPledgesCount = pledges.Where(d => !string.IsNullOrEmpty(d.Error)).Count();             

            //WARNING: Union the plegdes
            campaign.AllPledges = new List<Pledge>();
            campaign.AllPledges.AddRange(campaign.PendingApprovalPledges); 
            campaign.AllPledges.AddRange(campaign.FulfilledPledges); 

            campaign.Donors = donors.OrderByDescending(d => d.Amount).Take(campaign.LastItemsCount).ToList(); 
            campaign.DonorsCount = pledges.Select(d => d.UserName).Distinct().Count();

            campaign.Periods = GetPledgePeriods(campaign, pledges);

            if (campaign.Behavior.AutoDeactivateWhenGoalReached && 
                campaign.Fund >= campaign.Goal)
            {
                await UnregisterTimerAsync(EXTERNALIZE_TIMER_NAME);
                campaign.IsActive = false;  
            }

            Logger.LogInformation($"CampaignActor - RunPostPledgeProcessors [{this.Id.ToString()}] - {campaign.PendingApprovalPledges.Count}...");
            await this.SaveCampaignState(campaign);
            Logger.LogInformation($"CampaignActor - RunPostPledgeProcessors [{this.Id.ToString()}] - Saving campaign pledges...");
            await this.SavePledgesState(pledges);
            Logger.LogInformation($"CampaignActor - RunPostPledgeProcessors [{this.Id.ToString()}] - Saving campaign donors...");
            await this.SaveDonorsState(donors);

            Logger.LogInformation($"CampaignActor - RunPostPledgeProcessors [{this.Id.ToString()}] - Saving saving pldge and donor...");
            //WARNING: Saving to external store right away so we can keep pledges and donors readuly available
            if (pledge != null)
            {
                await this._daprClient.SaveStateAsync<Pledge>(Constants.DAPR_CAMPAIGNS_STORE_NAME, pledge.Identifier, pledge);
            }

            if (donor != null )
            {
                await this._daprClient.SaveStateAsync<Donor>(Constants.DAPR_CAMPAIGNS_STORE_NAME, donor.Identifier, donor);
            }
        } 
        catch (Exception e)
        {
            // Log the error....but do not re-throw
            Logger.LogError($"CampaignActor - RunPostPledgeProcessors [{this.Id.ToString()}] - caused a failure: {e.Message}");
        }
    }

    private async Task<Campaign> GetCampaignState() 
    {
        Campaign campaign = new Campaign();
        var actorState = await this.StateManager.TryGetStateAsync<Campaign>(CAMPAIGN_STATE_NAME);
        if (!actorState.HasValue) 
        {
            Logger.LogInformation($"CampaignActor - GetCampaignState [{this.Id.ToString()}]");
            var stateEntry = await _daprClient.GetStateEntryAsync<Campaign>(Constants.DAPR_CAMPAIGNS_STORE_NAME, this.Id.ToString());
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

        //WARNING: Save to external storage right away...duplicated in externalization
        campaign.LastRefreshTime = DateTime.Now;
        await this._daprClient.SaveStateAsync<Campaign>(Constants.DAPR_CAMPAIGNS_STORE_NAME, campaign.Identifier, campaign);

        //WARNING: Externalize campaign to SignalR to allow for real-time updates
        Logger.LogInformation($"CampaignActor - Externalize campaign [{this.Id.ToString()}] to SignalR for real-time updates....");
        var payload = new PayloadMessage();
        payload.Target = "UpdateCampaign";
        payload.Arguments = new[] { campaign };
        await _signalRService.CallViaRest("CampaignHub", payload);    
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

    private List<FundSinkPeriod> GetPledgePeriods(Campaign campaign, List<Pledge> pledges) 
    {
        Logger.LogInformation($"CampaignActor - GetPledgePeriods [{this.Id.ToString()}]");
        var now = DateTime.Now;
        var past = DateTime.Now;
        if (campaign.PeriodType == FundSinkPeriodTypes.Minute)
        {
            past = now.AddMinutes(-1 * campaign.PeriodsCount);
        }
        else
        {
            past = now.AddHours(-1 * campaign.PeriodsCount);
        }

        Logger.LogInformation($"CampaignActor - GetPledgePeriods total pledges: {pledges.Count()}");
        
        var actualPeriods = pledges.Where(p => 
            p.PledgeTime >= past
        )
        .GroupBy(p =>
        {
            TimeSpan ts = now - p.PledgeTime;
            int period = 0;

            if (campaign.PeriodType == FundSinkPeriodTypes.Minute)
            {
                period = (int)ts.TotalMinutes;
            }
            else
            {
                period = (int)ts.TotalHours;
            }

            return period;
        })
        .Select(g => new FundSinkPeriod { 
            Period = g.Key == 0 ? $"{g.Key}" : $"-{g.Key}", 
            Amount = g.Sum(s => s.Amount), 
            Count = g.Count() 
            })
        .ToList();             
    
        Logger.LogInformation($"CampaignActor - GetPledgePeriods last periods grouped pledges: {actualPeriods.Count()}");
        var allPeriods = new List<FundSinkPeriod>();
		for (int i = 0; i < campaign.PeriodsCount; i++)
		{
			allPeriods.Add(new FundSinkPeriod{ Period = (i == 0 ? $"{i}" : $"-{i}"), Amount = 0, Count = 0});
		}

		//TODO: Use Linq Join Group
		foreach (FundSinkPeriod period in allPeriods)
		{
            Logger.LogInformation($"CampaignActor - GetPledgePeriods period: {period.Period}");
			var actualPeriod = actualPeriods.Where(p => p.Period == period.Period).FirstOrDefault();
			if (actualPeriod != null) 
			{
                Logger.LogInformation($"CampaignActor - GetPledgePeriods matching pledge: {actualPeriod.Period}");
				period.Count = actualPeriod.Count;
				period.Amount = actualPeriod.Amount;
			}
		}

        allPeriods.Reverse();

        return allPeriods;
    }
}