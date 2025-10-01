using System;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Identity.Client;
using Microsoft.Identity.Client.Broker;
using Serilog;

namespace dotnet_sso_wpf_probe
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Configure Serilog to write to the console
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console(outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
                .CreateLogger();

            // Redirect Console output to the UI textbox so Serilog Console sink appears in the TextBox
            Console.SetOut(new TextBoxWriter(LogTextBox));
            Log.Information("Console output redirected to UI");

            Log.Information("UI initialized");

            this.Closed += MainWindow_Closed;
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            Log.Information("Login clicked");

            var scopes = new[] { "User.Read" };

            BrokerOptions options = new BrokerOptions(BrokerOptions.OperatingSystems.Windows);
            options.Title = "My Awesome Application";

            var wih = new System.Windows.Interop.WindowInteropHelper(this);
            var hWnd = wih.Handle;
            
            IPublicClientApplication app =
                PublicClientApplicationBuilder.Create("YOUR_CLIENT_ID")
                    .WithDefaultRedirectUri()
                    .WithParentActivityOrWindow(() => hWnd)
                    .WithBroker(options)
                    .Build();

            AuthenticationResult result = null;
            
            // Try to use the previously signed-in account from the cache
            IEnumerable<IAccount> accounts = await app.GetAccountsAsync();
            IAccount existingAccount = accounts.FirstOrDefault();

            try
            {    
                if (existingAccount != null)
                {
                    result = await app.AcquireTokenSilent(scopes, existingAccount).ExecuteAsync();
                }
                // Next, try to sign in silently with the account that the user is signed into Windows
                else
                {    
                    result = await app.AcquireTokenSilent(scopes, PublicClientApplication.OperatingSystemAccount)
                        .ExecuteAsync();
                }
            }
// Can't get a token silently, go interactive
            catch (MsalUiRequiredException ex)
            {
                try
                {
                    result = await app.AcquireTokenInteractive(scopes).ExecuteAsync();
                } catch (Exception iex)
                {
                    Log.Error("Interactive authentication failed: {Error}", iex.Message);
                    StatusTextBlock.Text = "Authentication failed";
                    return;
                }
            }
            catch (Exception ex)
            {
                Log.Error("Authentication failed: {Error}", ex.Message);
                StatusTextBlock.Text = "Authentication failed";
                return;
            }

            StatusTextBlock.Text = "Logged in";
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            StatusTextBlock.Text = "Logged out";
            Log.Information("Logout clicked");
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            Log.Information("Application closing");
            Log.CloseAndFlush();
        }
    }
}