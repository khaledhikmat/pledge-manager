using Dapr.Client;
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
public class CosmosPersistenceService : PersistenceService
{
    public CosmosPersistenceService(DaprClient daprClient, IEnvironmentService envService) 
    : base(daprClient, envService)
    {
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
}