using System.IO;
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