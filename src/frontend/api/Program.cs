using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.IdentityModel.Tokens;
using pledgemanager.frontend.api.Hubs;
using pledgemanager.frontend.api.Services;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

//Register an HTTP client to access signalr REST APIs directly
builder.Services.AddHttpClient("signalr");

//TODO: Once we daperize this API, there will be no need to do this
//We can use the dapr client directly

//Register an HTTP client to access the campaigns backend APIs directly
builder.Services.AddHttpClient("campaignbackend");

//Register an HTTP client to access the users backend APIs directly
builder.Services.AddHttpClient("usersbackend");

// Add services to the container.
builder.Services.AddSingleton<IConfiguration>(configuration);
builder.Services.AddSingleton<SignalRAuthService>(_ => new SignalRAuthService("EEndpoint=https://my-service.service.signalr.net;"));
builder.Services.AddSingleton<SignalRRestService>();
builder.Services.AddSingleton<ISettingService, SettingService>();
builder.Services.AddSingleton<IEntitiesService, EntitiesService>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = configuration["JwtIssuer"],
        ValidAudience = configuration["JwtAudience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JwtSecurityKey"]))
    };
});

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

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapControllers();
app.MapHub<ChatHub>("/chathub");
app.MapHub<CampaignHub>("/campaignhub");
app.MapHub<PledgeHub>("/pledgehub");
app.MapFallbackToFile("index.html");

app.Run();
