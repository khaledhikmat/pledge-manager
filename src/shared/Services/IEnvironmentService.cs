namespace pledgemanager.shared.Services;

public interface IEnvironmentService 
{
    string GetTargetEnvironment();
    bool IsDevEnvironment();
    string GetProduct();
    string GetCosmosConnectionString();
    string GetSignalRConnectionString();
    string GetCampaignsAppName();
    string GetFunctionsAppName();
    string GetStateStoreName();
    string GetPubSubName();
    string [] GetAllowedOrigins();
    string GetBaseUrl(string app);
}
