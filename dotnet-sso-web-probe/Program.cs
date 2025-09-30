using Microsoft.Extensions.Options;
using Serilog;
using dotnet_sso_web_probe;

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

// bind Keycloak section from appsettings.json into options
builder.Services.Configure<KeycloakOptions>(builder.Configuration.GetSection("Keycloak"));

builder.Services.AddSerilog(dispose: true);
var app = builder.Build();
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseRouting();
app.MapGet("/config", (IOptions<KeycloakOptions> opts) => Results.Json(new
{
    url = opts.Value.Url,
    realm = opts.Value.Realm,
    clientId = opts.Value.ClientId
}));

await app.StartAsync();

Console.WriteLine("Press Enter to stop the server...");
Console.ReadLine();

await app.StopAsync();