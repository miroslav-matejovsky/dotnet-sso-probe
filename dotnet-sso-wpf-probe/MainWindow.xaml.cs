using Serilog;

namespace dotnet_sso_wpf_probe;

using System;
using System.Threading.Tasks;

public partial class MainWindow
{
    public MainWindow()
    {
        InitializeComponent();
        Closed += async (_, _) =>
        {
            // stop embedded web app
            await App.StopWebApplicationAsync();

            // detach GUI writer callback to avoid references after window closed
            if (App.tw is GuiTextWriter gw)
            {
                gw.OnWrite = null;
            }
        };

        Log.Logger.Information("Starting application...");
        Log.Logger.Information("Base URL: {BaseUrl}", App.BaseUrl);
        CurrentUserText.Text = WindowsIdentityHelper.GetWindowsIdentityInfo();

        // Wire Serilog TextWriter sink to the SerilogSinkBox in the UI.
        if (App.tw is GuiTextWriter guiWriter)
        {
            guiWriter.OnWrite = text =>
            {
                try
                {
                    Dispatcher.Invoke(() =>
                    {
                        SerilogSinkBox.AppendText(text);
                        SerilogSinkBox.ScrollToEnd();
                    });
                }
                catch
                {
                    // if UI is unavailable, swallow to avoid throwing on background logging
                }
            };
        }

        OidcButton.Click += (_, _) => Log.Logger.Information("Not implemented");
        EditConfigButton.Click += (_, _) => Log.Logger.Information("Not implemented");
    }
}