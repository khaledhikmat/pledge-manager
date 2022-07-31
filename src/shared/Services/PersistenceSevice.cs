using Dapr.Client;
using pledgemanager.shared.Models;
using pledgemanager.shared.Utils;

namespace pledgemanager.shared.Services;

public class PersistenceService : IPersistenceService
{
    protected List<User> _users = new();
    protected List<Campaign> _campaigns = new();
    protected List<Institution> _institutions = new();
    protected List<FundSink> _fundSinks = new();
    protected List<Pledge> _pledges = new();
    protected List<Donor> _donors = new();

    protected readonly DaprClient _daprClient;
    protected readonly IEnvironmentService _environmentService;

    public PersistenceService(DaprClient daprClient, IEnvironmentService envService)
    {
        _daprClient = daprClient;
        _environmentService = envService;

        LoadSampleData();
    }

    public Task<User> RetrieveUserById(string id)
    {
        return Task.FromResult(_users.FirstOrDefault(c => c.Identifier == id));    
    }

    public Task<List<User>> RetrieveUsers()
    {
        return Task.FromResult<List<User>>(_users);
    }

    public Task<List<User>> RetrieveDonorUsers()
    {
        return Task.FromResult(_users.Where(d => d.Type == UserTypes.Donor).Select(d => d).ToList());    
    }

    public async Task PersistUser(User user)
    {
        await _daprClient.PublishEventAsync(_environmentService.GetPubSubName(), Constants.DAPR_USERS_PERSISTOR_PUBSUB_TOPIC_NAME, user);
    }

    public Task<Campaign> RetrieveCampaignById(string id)
    {
        return Task.FromResult(_campaigns.FirstOrDefault(c => c.Identifier == id));    
    }

    public Task<List<Campaign>> RetrieveCampaigns()
    {
        return Task.FromResult<List<Campaign>>(_campaigns);
    }

    public Task<Institution> RetrieveInstitutionById(string id)
    {
        var institution = _institutions.FirstOrDefault(c => c.Identifier == id);
        if (institution == null)
        {
            throw new InvalidOperationException($"Institution [{id}] not found");
        }

        return Task.FromResult(institution);    
    }

    public Task<List<Institution>> RetrieveInstitutions()
    {
        return Task.FromResult<List<Institution>>(_institutions);
    }

    public Task<FundSink> RetrieveFundSinkById(string id)
    {
        var fundSink = _fundSinks.FirstOrDefault(c => c.Identifier == id);
        if (fundSink == null)
        {
            throw new InvalidOperationException($"FundSink [{id}] not found");
        }

        return Task.FromResult(fundSink);    
    }

    public Task<List<FundSink>> RetrieveFundSinks()
    {
        return Task.FromResult<List<FundSink>>(_fundSinks);
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

    public Task SaveUser(User user)
    {
        return Task.FromResult<User>(default);
    }

    public Task SaveCampaign(Campaign campaign)
    {
        return Task.FromResult<Campaign>(default);
    }

    public Task SavePledge(Pledge pledge)
    {
        return Task.FromResult<Pledge>(default);
    }

    public Task SaveDonor(Donor donor)
    {
        return Task.FromResult<Donor>(default);
    }

    public Task SaveInstitution(Institution institution)
    {
        return Task.FromResult<Institution>(default);
    }

    public Task SaveFundSink(FundSink fundSink)
    {
        return Task.FromResult<FundSink>(default);
    }

    private void LoadSampleData() 
    {
        _fundSinks = new List<FundSink> {
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

        _institutions = new List<Institution> {
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

        _users = new List<User> {
            new User() {
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
            new User() {
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
            new User() {
                Identifier = "2105551200", 
                Type = UserTypes.Donor, 
                VerificationMethod = UserVerificationMethods.Sms, 
                UserName = "2105551200", 
                Name = "Abou Ya3goob",
                NickName = "Abou Ya3goob",
                Phone = "2105551200",
                Email = "2105551200@sat.com"
            },
            new User() {
                Identifier = "2105551201", 
                Type = UserTypes.Donor, 
                VerificationMethod = UserVerificationMethods.Sms, 
                UserName = "2105551201", 
                Name = "Abou Mazen",
                NickName = "Abou Mazen",
                Phone = "2105551201",
                Email = "2105551201@sat.com"
            },
            new User() {
                Identifier = "2105551202", 
                Type = UserTypes.Donor, 
                VerificationMethod = UserVerificationMethods.Sms, 
                UserName = "2105551202", 
                Name = "Sarraj Hassan",
                NickName = "Sarraj Hassan",
                Phone = "2105551202",
                Email = "2105551202@sat.com"
            },
            new User() {
                Identifier = "2105551203", 
                Type = UserTypes.Donor, 
                VerificationMethod = UserVerificationMethods.Sms, 
                UserName = "2105551203", 
                Name = "Mohammad Ali",
                NickName = "Mohammed Ali",
                Phone = "2105551203",
                Email = "2105551203@sat.com"
            },
            new User() {
                Identifier = "2105551204", 
                Type = UserTypes.Donor, 
                VerificationMethod = UserVerificationMethods.Sms, 
                UserName = "2105551204", 
                Name = "Noor Akram",
                NickName = "Noor Akram",
                Phone = "2105551204",
                Email = "2105551204@sat.com"
            },
            new User() {
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

        _campaigns = new List<Campaign> {
            new Campaign() {
                Identifier = "CAMP-00001", 
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
                    CampaignIdentifier = "CAMP-00001",
                    PledgeMode = CampaignPledgeModes.AutoApproval,
                    RestrictedPledgeAmounts = new List<double> {
                    },
                    AutoDeactivateWhenGoalReached = false,
                    MatchSupported = false
                }
            },
            new Campaign() {
                Identifier = "CAMP-00002", 
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
                Goal = 20000,
                Behavior = new CampaignBehavior() {
                    CampaignIdentifier = "CAMP-00002",
                    PledgeMode = CampaignPledgeModes.ManualApproval,
                    RestrictedPledgeAmounts = new List<double> {
                        100,250,500,1000
                    },
                    AutoDeactivateWhenGoalReached = false,
                    MatchSupported = false
                }
            },
            new Campaign() {
                Identifier = "CAMP-00003", 
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
                    CampaignIdentifier = "CAMP-00003",
                    PledgeMode = CampaignPledgeModes.HybridApproval,
                    AutoApprovePledgeIfAmountLE = 500,
                    AutoApprovePledgeIfAnonymous = true,
                    AutoDeactivateWhenGoalReached = true,
                    MatchSupported = true
                }
            },
            new Campaign() {
                Identifier = "CAMP-00004", 
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
                    CampaignIdentifier = "CAMP-00004",
                    PledgeMode = CampaignPledgeModes.HybridApproval,
                    AutoApprovePledgeIfAmountLE = 500,
                    AutoApprovePledgeIfAnonymous = true,
                    AutoDeactivateWhenGoalReached = true,
                    MatchSupported = true
                }
            }
        };
    }
}