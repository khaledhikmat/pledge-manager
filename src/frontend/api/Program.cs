using Microsoft.AspNetCore.ResponseCompression;
using pledgemanager.frontend.api.Hubs;
using pledgemanager.frontend.api.Services;

var builder = WebApplication.CreateBuilder(args);

//Register an HTTP client to access signalr REST APIs directly
builder.Services.AddHttpClient("signalr");

//Register an HTTP client to access the backend APIs directly
builder.Services.AddHttpClient("backend");

// Add services to the container.
builder.Services.AddSingleton<SignalRAuthService>(_ => new SignalRAuthService("Endpoint=https://signalrservice"));
builder.Services.AddSingleton<SignalRRestService>();
builder.Services.AddSingleton<ISettingService, SettingService>();
builder.Services.AddSingleton<IEntitiesService, EntitiesService>();

builder.Services.AddSignalR().AddAzureSignalR();
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddResponseCompression(opts =>
{
    opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
        new[] { "application/octet-stream" });
});

var app = builder.Build();
app.UseResponseCompression();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseRouting();

app.MapRazorPages();
app.MapControllers();
app.MapHub<ChatHub>("/chathub");
app.MapHub<CampaignHub>("/campaignhub");
app.MapFallbackToFile("index.html");

app.Run();
