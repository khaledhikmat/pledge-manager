using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

using pledgemanager.frontend.api.Services;

namespace pledgemanager.frontend.api
{
    public class Program
    {
        public static void Main()
        {
            var host = new HostBuilder()
            .ConfigureFunctionsWorkerDefaults()
            .ConfigureServices(s =>
            {
                s.AddHttpClient();
                s.AddSingleton<ISettingService, SettingService>();
                s.AddSingleton<IEntitiesService, EntitiesService>();
            })
            .Build();
            host.Run();
        }
    }
}