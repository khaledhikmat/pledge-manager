namespace pledgemanager.shared.Services;

using shared.Models;

public interface IPersistenceService 
{
    public Task LoadSampleData();
    public Task<User> RetrieveUserById(string id);
    public Task<List<User>> RetrieveUsers();
    public Task<List<User>> RetrieveDonorUsers();
    public Task<Campaign> RetrieveCampaignById(string id);
    public Task<List<Campaign>> RetrieveCampaigns();
    public Task<Institution> RetrieveInstitutionById(string id);
    public Task<List<Institution>> RetrieveInstitutions();
    public Task<FundSink> RetrieveFundSinkById(string id);
    public Task<List<FundSink>> RetrieveFundSinks();

    // The following `Persist` methods are called by actors
    // They only enqueue ...they don't actually save
    public Task PersistUser(User user);
    public Task PersistCampaign(Campaign campaign);
    public Task PersistPledge(Pledge pledge);
    public Task PersistDonor(Donor donor);
    public Task PersistInstitution(Institution institution);
    public Task PersistFundSink(FundSink fundSink);

    // The following `Save` methods are called by background processors to perform actual saves
    // and handle any denormalization or materialized views
    public Task SaveUser(User user);
    public Task SaveCampaign(Campaign campaign);
    public Task SavePledge(Pledge pledge);
    public Task SaveDonor(Donor donor);
    public Task SaveInstitution(Institution institution);
    public Task SaveFundSink(FundSink fundSink);
}