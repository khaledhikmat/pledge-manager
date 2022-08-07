using pledgemanager.shared.Utils;

namespace pledgemanager.shared.Services;

public class EnvironmentService : IEnvironmentService
{
    public string GetTargetEnvironment()
    {
        return Environment.GetEnvironmentVariable(Constants.TARGET_ENV) ?? "";
    }

    public bool IsDevEnvironment()
    {
        if (GetTargetEnvironment().ToLower() == "dev" ||
            GetTargetEnvironment().ToLower() == "local") 
        {
            return true;
        }

        return false;
    }

    public string GetProduct()
    {
        return Environment.GetEnvironmentVariable(Constants.PRODUCT) ?? "";

    }

    public string GetSignalRConnectionString()
    {
        return Environment.GetEnvironmentVariable(Constants.SIGNALR_CONN_STRING_ENV_VAR) ?? "";
    }

    public string GetCosmosConnectionString()
    {
        return Environment.GetEnvironmentVariable(Constants.COSMOS_CONN_STRING_ENV_VAR) ?? "";
    }

    public string GetCampaignsAppName()
    {
        return $"{GetProduct()}-{GetTargetEnvironment()}-campaigns";
    }

    public string GetFunctionsAppName()
    {
        return $"{GetProduct()}-{GetTargetEnvironment()}-functions";
    }

    public string GetStateStoreName()
    {
        //return $"{GetProduct()}-{GetTargetEnvironment()}-statestore}}";
        return Environment.GetEnvironmentVariable(Constants.DAPR_STATE_STORE_NAME_ENV_VAR) ?? "";
    }

    public string GetPubSubName()
    {
        //return $"{GetProduct()}-{GetTargetEnvironment()}-pubsub}}";
        return Environment.GetEnvironmentVariable(Constants.DAPR_PUBSUB_NAME_ENV_VAR) ?? "";
    }
    public string [] GetAllowedOrigins()
    {
        List<string> origins = new();
        if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable(Constants.ALLOWED_ORIGINS_ENV_VAR))) 
        {
            return origins.ToArray<string>();
        }

        origins = Environment.GetEnvironmentVariable(Constants.ALLOWED_ORIGINS_ENV_VAR).Split(',').Select(s => s).ToList();
        return origins.ToArray<string>();
    }
    public string GetBaseUrl(string app)
    {
        return Environment.GetEnvironmentVariable($"{app}_BASE_URL") ?? "";
    }
}