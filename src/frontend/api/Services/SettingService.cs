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

        public string GetBackendBaseUrl()
        {
            return _cfg.GetValue<string>("BackendBaseUrl");
        }
    }
}
