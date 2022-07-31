namespace pledgemanager.shared.Services;

public interface IEnvironmentService 
{
    string GetTargetEnvironment();
    string GetProduct();
    string GetSignalRConnectionString();
    string GetCampaignsAppName();
    string GetFunctionsAppName();
    string GetStateStoreName();
    string GetPubSubName();
    string [] GetAllowedOrigins();
    string GetBaseUrl(string app);
}
