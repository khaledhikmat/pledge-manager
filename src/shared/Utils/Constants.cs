namespace pledgemanager.shared.Utils;

public class Constants
{
    // ENV Varivables
    public const string TARGET_ENV = "TARGET_ENV";
    public const string PRODUCT = "PRODUCT";
    public const string SIGNALR_CONN_STRING_ENV_VAR = "SIGNALR_CONN_STRING";
    public const string ALLOWED_ORIGINS_ENV_VAR = "ALLOWED_ORIGINS";
    public const string DAPR_CAMPAIGNS_APP_NAME_ENV_VAR = "CAMPAIGNS_APP_NAME";
    public const string DAPR_USERS_APP_NAME_ENV_VAR = "USERS_APP_NAME";
    public const string DAPR_FUNCTIONS_APP_NAME_ENV_VAR = "FUNCTIONS_APP_NAME";
    public const string DAPR_STATE_STORE_NAME_ENV_VAR = "STATESTORE_NAME";
    public const string DAPR_PUBSUB_NAME_ENV_VAR = "PUBSUB_NAME";

    // CORS Policies
    public const string BLAZOR_POLICY = "BlazorPolicy";

    // PUB Topics
    public const string DAPR_USERS_PERSISTOR_PUBSUB_TOPIC_NAME = "users-persistor";
    public const string DAPR_FUNDSINKS_PERSISTOR_PUBSUB_TOPIC_NAME = "fundsinks-persistor";
    public const string DAPR_INSTITUTIONS_PERSISTOR_PUBSUB_TOPIC_NAME = "institutions-persistor";
    public const string DAPR_CAMPAIGNS_PERSISTOR_PUBSUB_TOPIC_NAME = "campaigns-persistor";
    public const string DAPR_CAMPAIGN_PLEDGES_PERSISTOR_PUBSUB_TOPIC_NAME = "campaign-pledges-persistor";
    public const string DAPR_CAMPAIGN_DONORS_PERSISTOR_PUBSUB_TOPIC_NAME = "campaign-donors-persistor";

    public const string DAPR_CAMPAIGN_PLEDGES_PROCESSOR_PUBSUB_TOPIC_NAME = "campaign-pledges-processor";
    public const string DAPR_CAMPAIGN_COMMANDS_PROCESSOR_PUBSUB_TOPIC_NAME = "campaign-commands-processor";

    //WARNING: Used mainly in campaigns microservice Processes controller...need a constant value
    public const string DAPR_PUBSUB_NAME = "pledgemanager-local-pubsub";

    // Others
    public const int USER_VALIDATION_PERIOD = 60;
    public const string VERIFICATION_TEMP_CODE = "123456";

}
