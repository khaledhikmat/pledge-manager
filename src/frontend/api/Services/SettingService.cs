using System;

namespace pledgemanager.frontend.api.Services
{
    public class SettingService : ISettingService
    {
        public string GetBackendBaseUrl()
        {
            return Environment.GetEnvironmentVariable("BackendBaseUrl");
        }
    }
}
