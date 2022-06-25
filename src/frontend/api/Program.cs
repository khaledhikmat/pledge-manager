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

//WARNING: The http factories are no longer needed since we r using DAPR.
//They are kept for refernce

//Register an HTTP client to access the campaigns backend APIs directly
builder.Services.AddHttpClient("campaignbackend");

//Register an HTTP client to access the users backend APIs directly
builder.Services.AddHttpClient("usersbackend");

// Add services to the container.
builder.Services.AddSingleton<IConfiguration>(configuration);
builder.Services.AddSingleton<SignalRAuthService>(_ => new SignalRAuthService("Endpoint=https://my-service.service.signalr.net;"));
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

var daprHttpPort = Environment.GetEnvironmentVariable("DAPR_HTTP_PORT") ?? "3602";
var daprGrpcPort = Environment.GetEnvironmentVariable("DAPR_GRPC_PORT") ?? "60002";
builder.Services.AddDaprClient(builder => builder
    .UseHttpEndpoint($"http://localhost:{daprHttpPort}")
    .UseGrpcEndpoint($"http://localhost:{daprGrpcPort}"));


//builder.Services.AddDaprClient();
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
    //app.UseHsts();
}

//app.UseHttpsRedirection();

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

//app.Run();
//Start
app.Run("http://0.0.0.0:6002");
