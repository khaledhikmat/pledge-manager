namespace pledgemanager.frontend.api.Services
{
    public interface ISettingService 
    {
        string GetCampaignsBackendBaseUrl();
        string GetUsersBackendBaseUrl();
    }
}