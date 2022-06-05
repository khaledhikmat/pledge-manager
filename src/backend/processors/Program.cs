var builder = WebApplication.CreateBuilder(args);

//Register an HTTP client to use signalr REST APIs directly
builder.Services.AddHttpClient("signalr");

// Add services to the container.
builder.Services.AddSingleton<SignalRAuthService>(_ => new SignalRAuthService("Endpoint=https://signalrservice"));
builder.Services.AddSingleton<SignalRRestService>();

builder.Services.AddControllers().AddDapr();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var daprHttpPort = Environment.GetEnvironmentVariable("DAPR_HTTP_PORT") ?? "3600";
var daprGrpcPort = Environment.GetEnvironmentVariable("DAPR_GRPC_PORT") ?? "60000";
builder.Services.AddDaprClient(builder => builder
    .UseHttpEndpoint($"http://localhost:{daprHttpPort}")
    .UseGrpcEndpoint($"http://localhost:{daprGrpcPort}"));

builder.Services.AddControllers();

builder.Services.AddActors(options =>
{
    options.Actors.RegisterActor<CampaignActor>();
    options.Actors.RegisterActor<InstitutionActor>();
    options.Actors.RegisterActor<RegionalActor>();
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage();
}
app.UseCloudEvents();

//app.UseHttpsRedirection();

//app.UseAuthorization();

app.MapControllers();
app.MapSubscribeHandler();
app.MapActorsHandlers();

//Start
app.Run("http://0.0.0.0:6000");
