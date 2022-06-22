using System;

namespace pledgemanager.frontend.api.Services
{
    public class SettingService : ISettingService
    {
        private IConfiguration _cfg;

        public SettingService(IConfiguration cfg) 
        {
            _cfg = cfg;
        }

        public string GetCampaignsBackendBaseUrl()
        {
            return _cfg.GetValue<string>("CampaignsBackendBaseUrl");
        }
        public string GetUsersBackendBaseUrl()
        {
            return _cfg.GetValue<string>("UsersBackendBaseUrl");
        }
    }
}
