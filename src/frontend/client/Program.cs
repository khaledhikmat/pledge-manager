using client;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Blazored.LocalStorage;
using BlazorComponentBus;
using Radzen;
using pledgemanager.client.Providers;
using Microsoft.AspNetCore.Components.Authorization;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Read configuration to discover the backend/functions API
var functionsApiUrl = builder.Configuration["backendUrl"];
if (string.IsNullOrEmpty(functionsApiUrl))
{
    functionsApiUrl = builder.HostEnvironment.BaseAddress;
}
builder.Services.AddScoped(sp => new HttpClient { 
    BaseAddress = new Uri(functionsApiUrl) 
});
Console.WriteLine($"Functions API URL is: {functionsApiUrl}");

// Radzen Services
builder.Services.AddScoped<DialogService>();
builder.Services.AddScoped<NotificationService>();
builder.Services.AddScoped<TooltipService>();
builder.Services.AddScoped<ContextMenuService>();
builder.Services.AddScoped<ComponentBus>();
builder.Services.AddBlazoredLocalStorage();

builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<TokenAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(provider => 
    provider.GetRequiredService<TokenAuthenticationStateProvider>());

await builder.Build().RunAsync();
