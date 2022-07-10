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

// Add services to the container.
builder.Services.AddSingleton<IConfiguration>(configuration);
builder.Services.AddSingleton<IEnvironmentService, EnvironmentService>();

var envService = builder.Services.BuildServiceProvider().GetService<IEnvironmentService>();

builder.Services.AddSingleton<SignalRAuthService>(sp => 
{
    return new SignalRAuthService(envService.GetSignalRConnectionString());
});
builder.Services.AddSingleton<SignalRRestService>();
builder.Services.AddSingleton<IEntitiesService, EntitiesService>();

var corsPlicyName = "BlazorPolicy";
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: Constants.BLAZOR_POLICY,
                      policy  =>
                      {
                          policy
                          .WithOrigins(envService.GetAllowedOrigins())
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials();
                      });
});


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

app.UseCors(corsPlicyName);

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
