namespace pledgemanager.shared.Utils;

public class Constants
{
    public const string DAPR_CAMPAIGNS_STORE_NAME = "campaigns-statestore";
    public const string DAPR_CAMPAIGNS_PUBSUB_NAME = "campaigns-pubsub";
    public const string DAPR_PLEDGES_PUBSUB_TOPIC_NAME = "pledges";
    public const string DAPR_COMMANDS_PUBSUB_TOPIC_NAME = "commands";

    public const string DAPR_USERS_STORE_NAME = "users-statestore";
    public const string DAPR_USERS_PUBSUB_NAME = "users-pubsub";

    public const int USER_VALIDATION_PERIOD = 60;
    public const string VERIFICATION_TEMP_CODE = "123456";

}
