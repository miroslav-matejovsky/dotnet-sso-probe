using System.IO;
using System.Text;
using System.Windows;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Serilog;

namespace dotnet_sso_wpf_probe;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App
{
    // Use our GUI-capable writer implementation so Serilog can write into the UI.
    public static readonly TextWriter tw = new GuiTextWriter();
    private static WebApplication? _application;
    internal static string BaseUrl { get; private set; } = "not-started"; // default

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        var builder = WebApplication.CreateBuilder(e.Args);
        builder.Services.AddSerilog((_, loggerConfiguration) => loggerConfiguration
            .ReadFrom.Configuration(builder.Configuration)
            .WriteTo.TextWriter(tw, outputTemplate: "{Timestamp:HH:mm:ss} {Message:lj}{NewLine}"));
        
        var app = builder.Build();
        app.UseDefaultFiles();
        app.UseStaticFiles();
        app.UseRouting();
        // Local handler used for both root and catch-all routes.
        IResult HandleGet(HttpContext ctx)
        {
            var path = ctx.Request.Path.HasValue ? ctx.Request.Path.Value! : "/";
            var queryDict = ctx.Request.Query.ToDictionary(k => k.Key, v => (string)v.Value);
            var sb = new StringBuilder();
            sb.AppendLine($"Received GET {path}{ctx.Request.QueryString}");
            if (queryDict.Count == 0)
            {
                sb.AppendLine("(no query parameters)");
            }
            else
            {
                foreach (var kv in queryDict)
                {
                    sb.AppendLine($"{kv.Key} = {kv.Value}");
                }
            }

            // Log into the GUI writer.
            tw.WriteLine(sb.ToString());

            // Return the same info as JSON.
            return Results.Bytes("ok"u8.ToArray(), "text/plain");
        }
        
        // Map root
        app.MapGet("/", (HttpContext ctx) => HandleGet(ctx));
        // Map any other GET request (catch-all)
        app.MapGet("/{**catchall}", (HttpContext ctx) => HandleGet(ctx));

        // Start the app so server features (including addresses) are populated.
        app.StartAsync().GetAwaiter().GetResult();
        // Get bound addresses from the server features.
        var server = app.Services.GetRequiredService<IServer>();
        var addressesFeature = server.Features.Get<IServerAddressesFeature>();
        var firstAddress = addressesFeature?.Addresses?.FirstOrDefault() ?? app.Urls.FirstOrDefault();
        if (!string.IsNullOrEmpty(firstAddress))
        {
            BaseUrl = firstAddress!;
        }
        _application = app;
    }

    protected override void OnExit(ExitEventArgs e)
    {
        StopWebApplicationAsync().GetAwaiter().GetResult();
        base.OnExit(e);
    }
    
    
    
    public static async Task StopWebApplicationAsync()
    {
        if (_application != null)
        {
            await _application.StopAsync();
        }
    }
}