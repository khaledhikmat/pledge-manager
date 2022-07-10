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
    public const string DAPR_PLEDGES_PUBSUB_TOPIC_NAME = "pledges";
    public const string DAPR_COMMANDS_PUBSUB_TOPIC_NAME = "commands";

    //WARNING: Used mainly in campagns microservice Processes controller...need a constant value
    public const string DAPR_PUBSUB_NAME = "pledgemanager-local-pubsub";

    // Others
    public const int USER_VALIDATION_PERIOD = 60;
    public const string VERIFICATION_TEMP_CODE = "123456";

}
