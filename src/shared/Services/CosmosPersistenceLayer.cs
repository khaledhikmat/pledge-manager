using Dapr.Client;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using pledgemanager.shared.Models;
using pledgemanager.shared.Utils;

namespace pledgemanager.shared.Services;

/*
This Cosmos service deals with persisting data to Cosmos. No Cosmos-specific 
elements should seep into other parts of the application.

As such, it inherits the base `PersistenceService` and overrides the 
`Retrieve*` and `Save*` methods.

Microservices that require Cosmos persistence capabilities can change the persistence 
implementation in `Program.cs`: 

FROM: 
builder.Services.AddSingleton<IPersistenceService, PersistenceService>();

TO:
builder.Services.AddSingleton<IPersistenceService, CosmosPersistenceService>();
*/
public class CosmosPersistenceService : IPersistenceService
{
    private const string PLEDGE_MANAGER_DB_NAME = "PledgeManagerDb";
    private const string CAMPAIGNS_CONTAINER_NAME = "Campaign";
    private const string USERS_CONTAINER_NAME = "User";
    private const string META_CONTAINER_NAME = "Meta";
    static SemaphoreSlim LOCK = new SemaphoreSlim(1,1);

    private DaprClient _daprClient;
    private IEnvironmentService _environmentService;
    private ILogger<CosmosPersistenceService> _logger;
    private CosmosClient _client;
    private bool _initialized = false;
    private bool _seeded = false;

    public CosmosPersistenceService(DaprClient daprClient, IEnvironmentService envService, ILogger<CosmosPersistenceService> logger) 
    {
        _daprClient = daprClient;
        _environmentService = envService;
        _logger = logger;
        _client = null;
    }

    public async Task LoadSampleData() 
    {
        if (this._environmentService.IsDevEnvironment() &&
            !this._seeded)
        {
            _logger.LogInformation($"LoadSampleData - seed");

            try
            {
                List<FundSink> fundSinks = new List<FundSink> {
                    new FundSink() {
                        Identifier = "Global", 
                        Type = FundSinkTypes.Global, 
                        Name = "Global", 
                        Currency = "USD"
                    },
                    new FundSink() {
                        Identifier = "USA", 
                        Type = FundSinkTypes.Country, 
                        Name = "USA", 
                        Currency = "USD"
                    },
                    new FundSink() {
                        Identifier = "TX", 
                        Type = FundSinkTypes.State, 
                        Name = "TX", 
                        Currency = "USD"
                    },
                    new FundSink() {
                        Identifier = "SAT", 
                        Type = FundSinkTypes.City, 
                        Name = "SAT", 
                        Currency = "USD"
                    },
                };

                List<Institution> institutions = new List<Institution> {
                    new Institution() {
                        Identifier = "INST-00001", 
                        Type = FundSinkTypes.Institution, 
                        Name = "ICSA", 
                        Title = "Islamic Center of San Antonio", 
                        Description = "Islamic Center of San Antonio serves the SAT community .....", 
                        ImageUrl = "https://picsum.photos/200/200",
                        Currency = "USD",
                        Country = "USA",
                        State = "TX",
                        City = "SAT"
                    },
                    new Institution() {
                        Identifier = "INST-00002", 
                        Type = FundSinkTypes.Institution, 
                        Name = "MCCC", 
                        Title = "Muslim Children and Civic Center", 
                        Description = "Muslim Children and Civic Center serves the SAT community .....", 
                        ImageUrl = "https://picsum.photos/200/200",
                        Currency = "USD",
                        Country = "USA",
                        State = "TX",
                        City = "SAT"
                    }
                };

                List<pledgemanager.shared.Models.User> users = new List<pledgemanager.shared.Models.User> {
                    new pledgemanager.shared.Models.User() {
                        Identifier = "admin@icsa.com", 
                        Type = UserTypes.Organizer, 
                        VerificationMethod = UserVerificationMethods.Sms, 
                        Permission = new UserPermission() {
                        Institutions = new Dictionary<string, string>() {
                                {"INST-00001", "ICSA"}  
                        } 
                        }, 
                        UserName = "admin@icsa.com", 
                        Name = "icsa-admin",
                        NickName = "icsa-admin",
                        Phone = "2107771212",
                        Email = "admin@icsa.com"
                    },
                    new pledgemanager.shared.Models.User() {
                        Identifier = "admin@meccc.com", 
                        Type = UserTypes.Organizer, 
                        VerificationMethod = UserVerificationMethods.Sms, 
                        Permission = new UserPermission() {
                        Institutions = new Dictionary<string, string>() {
                                {"INST-00002", "MCCC"}  
                        } 
                        }, 
                        UserName = "admin@meccc.com", 
                        Name = "meccc-admin",
                        NickName = "meccc-admin",
                        Phone = "2108881212",
                        Email = "admin@meccc.com"
                    },
                    new pledgemanager.shared.Models.User() {
                        Identifier = "2105551200", 
                        Type = UserTypes.Donor, 
                        VerificationMethod = UserVerificationMethods.Sms, 
                        UserName = "2105551200", 
                        Name = "Abou Ya3goob",
                        NickName = "Abou Ya3goob",
                        Phone = "2105551200",
                        Email = "2105551200@sat.com"
                    },
                    new pledgemanager.shared.Models.User() {
                        Identifier = "2105551201", 
                        Type = UserTypes.Donor, 
                        VerificationMethod = UserVerificationMethods.Sms, 
                        UserName = "2105551201", 
                        Name = "Abou Mazen",
                        NickName = "Abou Mazen",
                        Phone = "2105551201",
                        Email = "2105551201@sat.com"
                    },
                    new pledgemanager.shared.Models.User() {
                        Identifier = "2105551202", 
                        Type = UserTypes.Donor, 
                        VerificationMethod = UserVerificationMethods.Sms, 
                        UserName = "2105551202", 
                        Name = "Sarraj Hassan",
                        NickName = "Sarraj Hassan",
                        Phone = "2105551202",
                        Email = "2105551202@sat.com"
                    },
                    new pledgemanager.shared.Models.User() {
                        Identifier = "2105551203", 
                        Type = UserTypes.Donor, 
                        VerificationMethod = UserVerificationMethods.Sms, 
                        UserName = "2105551203", 
                        Name = "Mohammad Ali",
                        NickName = "Mohammed Ali",
                        Phone = "2105551203",
                        Email = "2105551203@sat.com"
                    },
                    new pledgemanager.shared.Models.User() {
                        Identifier = "2105551204", 
                        Type = UserTypes.Donor, 
                        VerificationMethod = UserVerificationMethods.Sms, 
                        UserName = "2105551204", 
                        Name = "Noor Akram",
                        NickName = "Noor Akram",
                        Phone = "2105551204",
                        Email = "2105551204@sat.com"
                    },
                    new pledgemanager.shared.Models.User() {
                        Identifier = "2105551205", 
                        Type = UserTypes.Donor, 
                        VerificationMethod = UserVerificationMethods.Sms, 
                        UserName = "2105551205", 
                        Name = "Sam Donaldson",
                        NickName = "Sam Donaldson",
                        Phone = "2105551205",
                        Email = "2105551205@sat.com"
                    }
                };

                List<Campaign> campaigns = new List<Campaign> {
                    new Campaign() {
                        Identifier = "CAMP-00001", 
                        CampaignIdentifier = "CAMP-00001", 
                        Type = FundSinkTypes.Campain, 
                        Institution = "ICSA", 
                        InstitutionIdentifier = "INST-00001", 
                        Name = "Masjed Expenses", 
                        Title = "ICSA Masjed Expenses", 
                        Description = "ICSA Masjed Expenses are very important to allow us to .....", 
                        ImageUrl = "https://picsum.photos/200/200",
                        Currency = "USD",
                        Start = DateTime.Now.AddDays(-10),
                        Stop = DateTime.Now.AddDays(10),
                        IsActive = true,
                        Goal = 50000,
                        Behavior = new CampaignBehavior() {
                            PledgeMode = CampaignPledgeModes.AutoApproval,
                            RestrictedPledgeAmounts = new List<double> {
                            },
                            AutoDeactivateWhenGoalReached = false,
                            MatchSupported = false
                        }
                    },
                    new Campaign() {
                        Identifier = "CAMP-00002", 
                        CampaignIdentifier = "CAMP-00002", 
                        Type = FundSinkTypes.Campain, 
                        Institution = "ICSA", 
                        InstitutionIdentifier = "INST-00001", 
                        Name = "Remodeling", 
                        Title = "Bathroom Remodeling", 
                        Description = "ICSA Bathroom Remodeling is a must due to the .....", 
                        ImageUrl = "https://picsum.photos/200/200",
                        Currency = "USD",
                        Start = DateTime.Now.AddDays(-10),
                        Stop = DateTime.Now.AddDays(10),
                        IsActive = true,
                        Goal = 30000,
                        Behavior = new CampaignBehavior() {
                            PledgeMode = CampaignPledgeModes.ManualApproval,
                            RestrictedPledgeAmounts = new List<double> {
                                100,200,500,1000,2000,5000,10000,25000
                            },
                            AutoDeactivateWhenGoalReached = true,
                            MatchSupported = false,
                            MinPledgeAmount = 50,
                            MaxPledgeAmount = 30000
                        }
                    },
                    new Campaign() {
                        Identifier = "CAMP-00003", 
                        CampaignIdentifier = "CAMP-00003", 
                        Type = FundSinkTypes.Campain, 
                        Institution = "ICSA", 
                        InstitutionIdentifier = "INST-00001", 
                        Name = "School", 
                        Title = "School Remodeling", 
                        Description = "ICSA School Remodeling is a must due to the .....", 
                        ImageUrl = "https://picsum.photos/200/200",
                        Currency = "USD",
                        Start = DateTime.Now.AddDays(-10),
                        Stop = DateTime.Now.AddDays(10),
                        IsActive = true,
                        Goal = 30000,
                        Behavior = new CampaignBehavior() {
                            PledgeMode = CampaignPledgeModes.HybridApproval,
                            AutoApprovePledgeIfAmountLE = 500,
                            AutoApprovePledgeIfAnonymous = true,
                            AutoDeactivateWhenGoalReached = true,
                            MatchSupported = true
                        }
                    },
                    new Campaign() {
                        Identifier = "CAMP-00004", 
                        CampaignIdentifier = "CAMP-00004", 
                        Type = FundSinkTypes.Campain, 
                        Institution = "MCCC", 
                        InstitutionIdentifier = "INST-00002", 
                        Name = "Gym", 
                        Title = "Gym Remodeling", 
                        Description = "MCCC Gym is a must due to the .....", 
                        ImageUrl = "https://picsum.photos/200/200",
                        Currency = "USD",
                        Start = DateTime.Now.AddDays(-10),
                        Stop = DateTime.Now.AddDays(10),
                        IsActive = true,
                        Goal = 40000,
                        Behavior = new CampaignBehavior() {
                            PledgeMode = CampaignPledgeModes.HybridApproval,
                            AutoApprovePledgeIfAmountLE = 500,
                            AutoApprovePledgeIfAnonymous = true,
                            AutoDeactivateWhenGoalReached = true,
                            MatchSupported = true
                        }
                    }
                };

                foreach(FundSink sink in fundSinks)
                {
                    _logger.LogInformation($"LoadSampleData - sink pk = {sink.PartitionKey} - id = {sink.Identifier}");
                    await SaveFundSink(sink);
                    _logger.LogInformation($"LoadSampleData - done sink");
                }

                foreach(Institution inst in institutions)
                {
                    _logger.LogInformation($"LoadSampleData - inst");
                    await SaveInstitution(inst);
                }

                foreach(pledgemanager.shared.Models.User user in users)
                {
                    _logger.LogInformation($"LoadSampleData - user");
                    await SaveUser(user);
                }

                foreach(Campaign campaign in campaigns)
                {
                    _logger.LogInformation($"LoadSampleData - campaign");
                    await SaveCampaign(campaign);
                }
            }
            catch (Exception e)
            {
                _logger.LogError($"LoadSampleData error: {e.Message}");
            }
            finally 
            {
                this._seeded = true;
            }
        }
    }

    public async Task<pledgemanager.shared.Models.User> RetrieveUserById(string id)
    {
        var container = await GetUsersContainer();
        return await container.ReadItemAsync<pledgemanager.shared.Models.User>(
        id,
        new PartitionKey(id));
    }

    public async Task<List<pledgemanager.shared.Models.User>> RetrieveUsers()
    {
        //WARNING: Cross-partition query for now
        List<pledgemanager.shared.Models.User> users = new List<pledgemanager.shared.Models.User>();
        var container = await GetUsersContainer();

        // Build query definition
        var parameterizedQuery = new QueryDefinition(
            query: "SELECT * FROM User u"
        );

        // Query multiple items from container
        using FeedIterator<pledgemanager.shared.Models.User> filteredFeed = container.GetItemQueryIterator<pledgemanager.shared.Models.User>(
            queryDefinition: parameterizedQuery
        );

        // Iterate query result pages
        while (filteredFeed.HasMoreResults)
        {
            FeedResponse<pledgemanager.shared.Models.User> response = await filteredFeed.ReadNextAsync();

            // Iterate query results
            foreach (pledgemanager.shared.Models.User item in response)
            {
                users.Add(item);
            }
        }

        return users;
    }

    public async Task<List<pledgemanager.shared.Models.User>> RetrieveDonorUsers()
    {
        //WARNING: Cross-partion query
        List<pledgemanager.shared.Models.User> users = new List<pledgemanager.shared.Models.User>();
        var container = await GetUsersContainer();

        // Build query definition
        var parameterizedQuery = new QueryDefinition(
            query: "SELECT * FROM User u WHERE u.Type = @type"
        ).WithParameter("@type", UserTypes.Donor);

        // Query multiple items from container
        using FeedIterator<pledgemanager.shared.Models.User> filteredFeed = container.GetItemQueryIterator<pledgemanager.shared.Models.User>(
            queryDefinition: parameterizedQuery
        );

        // Iterate query result pages
        while (filteredFeed.HasMoreResults)
        {
            FeedResponse<pledgemanager.shared.Models.User> response = await filteredFeed.ReadNextAsync();

            // Iterate query results
            foreach (pledgemanager.shared.Models.User item in response)
            {
                users.Add(item);
            }
        }

        return users;
    }

    public async Task<Campaign> RetrieveCampaignById(string id)
    {
        var container = await GetCampaignsContainer();
        return await container.ReadItemAsync<Campaign>(
        id,
        new PartitionKey(id));
    }

    public async Task<List<Campaign>> RetrieveCampaigns()
    {
        List<Campaign> campaigns = new List<Campaign>();
        var container = await GetMetaContainer();

        // Build query definition
        var parameterizedQuery = new QueryDefinition(
            query: "SELECT * FROM Meta m WHERE m.PartitionKey = @pk"
        ).WithParameter("@pk", FundSink.GetPartitionKeyByType(FundSinkTypes.Campain));

        // Query multiple items from container
        using FeedIterator<Campaign> filteredFeed = container.GetItemQueryIterator<Campaign>(
            queryDefinition: parameterizedQuery
        );

        // Iterate query result pages
        while (filteredFeed.HasMoreResults)
        {
            FeedResponse<Campaign> response = await filteredFeed.ReadNextAsync();

            // Iterate query results
            foreach (Campaign item in response)
            {
                campaigns.Add(item);
            }
        }

        return campaigns;
    }

    public async Task<Institution> RetrieveInstitutionById(string id)
    {
        var container = await GetMetaContainer();
        return await container.ReadItemAsync<Institution>(
        id,
        new PartitionKey(FundSink.GetPartitionKeyByType(FundSinkTypes.Institution)));
    }

    public async Task<List<Institution>> RetrieveInstitutions()
    {
        List<Institution> insts = new List<Institution>();
        var container = await GetMetaContainer();

        // Build query definition
        var parameterizedQuery = new QueryDefinition(
            query: "SELECT * FROM Meta m WHERE m.PartitionKey = @pk"
        ).WithParameter("@pk", FundSink.GetPartitionKeyByType(FundSinkTypes.Institution));

        // Query multiple items from container
        using FeedIterator<Institution> filteredFeed = container.GetItemQueryIterator<Institution>(
            queryDefinition: parameterizedQuery
        );

        // Iterate query result pages
        while (filteredFeed.HasMoreResults)
        {
            FeedResponse<Institution> response = await filteredFeed.ReadNextAsync();

            // Iterate query results
            foreach (Institution item in response)
            {
                insts.Add(item);
            }
        }

        return insts;
    }

    public async Task<FundSink> RetrieveFundSinkById(string id)
    {
        var container = await GetMetaContainer();
        return await container.ReadItemAsync<Institution>(
        id,
        new PartitionKey(FundSink.GetPartitionKeyByType(FundSinkTypes.Global)));
    }

    public async Task<List<FundSink>> RetrieveFundSinks()
    {
        List<FundSink> sinks = new List<FundSink>();
        var container = await GetMetaContainer();

        // Build query definition
        var parameterizedQuery = new QueryDefinition(
            query: "SELECT * FROM Meta m WHERE m.PartitionKey = @pk"
        ).WithParameter("@pk", FundSink.GetPartitionKeyByType(FundSinkTypes.Global));

        // Query multiple items from container
        using FeedIterator<FundSink> filteredFeed = container.GetItemQueryIterator<FundSink>(
            queryDefinition: parameterizedQuery
        );

        // Iterate query result pages
        while (filteredFeed.HasMoreResults)
        {
            FeedResponse<FundSink> response = await filteredFeed.ReadNextAsync();

            // Iterate query results
            foreach (FundSink item in response)
            {
                sinks.Add(item);
            }
        }

        return sinks;
    }

    public async Task PersistUser(pledgemanager.shared.Models.User user)
    {
        await _daprClient.PublishEventAsync(_environmentService.GetPubSubName(), Constants.DAPR_USERS_PERSISTOR_PUBSUB_TOPIC_NAME, user);
    }

    public async Task PersistCampaign(Campaign campaign)
    {
        await _daprClient.PublishEventAsync(_environmentService.GetPubSubName(), Constants.DAPR_CAMPAIGNS_PERSISTOR_PUBSUB_TOPIC_NAME, campaign);
    }

    public async Task PersistDonor(Donor donor)
    {
        await _daprClient.PublishEventAsync(_environmentService.GetPubSubName(), Constants.DAPR_CAMPAIGN_DONORS_PERSISTOR_PUBSUB_TOPIC_NAME, donor);
    }

    public async Task PersistPledge(Pledge pledge)
    {
        await _daprClient.PublishEventAsync(_environmentService.GetPubSubName(), Constants.DAPR_CAMPAIGN_PLEDGES_PERSISTOR_PUBSUB_TOPIC_NAME, pledge);
    }

    public async Task PersistInstitution(Institution institution)
    {
        await _daprClient.PublishEventAsync(_environmentService.GetPubSubName(), Constants.DAPR_INSTITUTIONS_PERSISTOR_PUBSUB_TOPIC_NAME, institution);
    }

    public async Task PersistFundSink(FundSink fundSink)
    {
        await _daprClient.PublishEventAsync(_environmentService.GetPubSubName(), Constants.DAPR_FUNDSINKS_PERSISTOR_PUBSUB_TOPIC_NAME, fundSink);
    }

    public async Task SaveUser(pledgemanager.shared.Models.User user)
    {
        var container = await GetUsersContainer();
        await container.UpsertItemAsync(user, new PartitionKey(user.Identifier));
    }

    public async Task SaveCampaign(Campaign campaign)
    {
        campaign.PartitionKey = FundSink.GetPartitionKey(campaign);
        var container = await GetCampaignsContainer();
        await container.UpsertItemAsync(campaign, new PartitionKey(campaign.Identifier));

        // Save to Meta to allow for partition queries
        var container2 = await GetMetaContainer();
        await container2.UpsertItemAsync(campaign, new PartitionKey(campaign.PartitionKey));
    }

    public async Task SavePledge(Pledge pledge)
    {
        var container = await GetCampaignsContainer();
        await container.UpsertItemAsync(pledge, new PartitionKey(pledge.CampaignIdentifier));
    }

    public async Task SaveDonor(Donor donor)
    {
        var container = await GetCampaignsContainer();
        await container.UpsertItemAsync(donor, new PartitionKey(donor.CampaignIdentifier));
    }

    public async Task SaveInstitution(Institution institution)
    {
        institution.PartitionKey = FundSink.GetPartitionKey(institution);
        var container = await GetMetaContainer();
        await container.UpsertItemAsync(institution, new PartitionKey(institution.PartitionKey));

        //TODO: Determine if name has changed and broadcast to campaigns to update denormalized elements
    }

    public async Task SaveFundSink(FundSink fundSink)
    {
        try
        {
            fundSink.PartitionKey = FundSink.GetPartitionKey(fundSink);
            var container = await GetMetaContainer();
            _logger.LogInformation($"SaveFundSink - sink pk = {fundSink.PartitionKey} - id = {fundSink.Identifier}");
            await container.UpsertItemAsync(fundSink, new PartitionKey(fundSink.PartitionKey));
        }
        catch (Exception e) 
        {
            _logger.LogError($"SaveFundSink - Error {e.Message} - {e.InnerException}");
        }
    }

    /* PRIVATE */
    private async Task<Container> GetCampaignsContainer() 
    {
        await InitializeDatabaseAsync();
        return _client.GetContainer(PLEDGE_MANAGER_DB_NAME, CAMPAIGNS_CONTAINER_NAME);
    }

    private async Task<Container> GetUsersContainer() 
    {
        await InitializeDatabaseAsync();
        return _client.GetContainer(PLEDGE_MANAGER_DB_NAME, USERS_CONTAINER_NAME);
    }

    private async Task<Container> GetMetaContainer() 
    {
        await InitializeDatabaseAsync();
        return _client.GetContainer(PLEDGE_MANAGER_DB_NAME, META_CONTAINER_NAME);
    }

    private async Task InitializeDatabaseAsync()
    {
        _logger.LogInformation($"InitializeDatabaseAsync - entry");
        if (this._initialized)
        {
            return;
        }

        _logger.LogInformation($"InitializeDatabaseAsync - init");
        await LOCK.WaitAsync();

        try
        {
            if (_client == null)
            {
                _client = new CosmosClient(this._environmentService.GetCosmosConnectionString());
            }

            var result = await _client.CreateDatabaseIfNotExistsAsync(PLEDGE_MANAGER_DB_NAME);
            await result.Database.CreateContainerIfNotExistsAsync(
                CAMPAIGNS_CONTAINER_NAME, "/CampaignIdentifier");
            await result.Database.CreateContainerIfNotExistsAsync(
                USERS_CONTAINER_NAME, "/id");
            await result.Database.CreateContainerIfNotExistsAsync(
                META_CONTAINER_NAME, "/PartitionKey");

            this._initialized = true;
        }
        finally
        {
            LOCK.Release();
        }
    }
}