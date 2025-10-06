using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Identity.Client;
using Microsoft.Identity.Client.Broker;
using Serilog;

namespace dotnet_sso_wpf_probe;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        // Configure Serilog to write to the console
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console(
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
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

        var result = await AuthenticateUserViaEntraId();
        if (result == null)
        {
            Log.Information("No EntraId token obtained");
            return;
        }
        StatusTextBlock.Text = "Logged in";

        // Log token info for successful login. Avoid logging the raw token value; log length instead.
        var username = result.Account?.Username ?? "(unknown)";
        var homeAccountId = result.Account?.HomeAccountId?.Identifier ?? "(unknown)";
        var expiresOn = result.ExpiresOn;
        var receivedScopes = result.Scopes != null ? string.Join(" ", result.Scopes) : "(none)";
        var accessTokenLength = result.AccessToken?.Length ?? 0;
        var hasIdToken = !string.IsNullOrEmpty(result.IdToken);

        Log.Information(
            "Authentication succeeded. Username: {Username}, HomeAccountId: {HomeAccountId}, ExpiresOn: {ExpiresOn:u}, Scopes: {Scopes}, AccessTokenLength: {AccessTokenLength}, HasIdToken: {HasIdToken}",
            username, homeAccountId, expiresOn, receivedScopes, accessTokenLength, hasIdToken);
        
        // Now perform token exchange with Keycloak
        await ExchangeTokenWithKeycloak(result);
        
    }

    private void LogoutButton_Click(object sender, RoutedEventArgs e)
    {
        StatusTextBlock.Text = "Logged out";
        Log.Information("Logout clicked");
    }

    private static void MainWindow_Closed(object? sender, EventArgs e)
    {
        Log.Information("Application closing");
        Log.CloseAndFlush();
    }

    private async Task<AuthenticationResult?> AuthenticateUserViaEntraId()
    {
        var clientId = ClientIdTextBox.Text?.Trim();
        if (string.IsNullOrEmpty(clientId))
        {
            Log.Error("Client ID is empty");
            StatusTextBlock.Text = "Please enter Client ID";
            return null;
        }

        var acquireScopes = new[] { "User.Read" };

        var brokerOptions = new BrokerOptions(BrokerOptions.OperatingSystems.Windows)
        {
            Title = "SSO WPF Probe"
        };

        // Read tenant ID from the UI; if empty, leave it null so MSAL uses the default/common tenant behaviour
        var tenantIdInput = TenantIdTextBox.Text?.Trim();
        string? tenantId = string.IsNullOrWhiteSpace(tenantIdInput) ? null : tenantIdInput;

        Log.Debug("Using TenantId: {TenantId}", tenantId ?? "(default)");

        var applicationOptions = new PublicClientApplicationOptions
        {
            TenantId = tenantId,
            ClientId = clientId
        };

        var wih = new System.Windows.Interop.WindowInteropHelper(this);
        var hWnd = wih.Handle;

        IPublicClientApplication app =
            PublicClientApplicationBuilder
                .CreateWithApplicationOptions(applicationOptions)
                .WithDefaultRedirectUri()
                .WithParentActivityOrWindow(() => hWnd)
                .WithBroker(brokerOptions)
                .Build();

        AuthenticationResult? result;

        // Try to use the previously signed-in account from the cache
        IEnumerable<IAccount> accounts = await app.GetAccountsAsync();
        IAccount? existingAccount = accounts.FirstOrDefault();
        try
        {
            if (existingAccount != null)
            {
                result = await app.AcquireTokenSilent(acquireScopes, existingAccount).ExecuteAsync();
            }
            // Next, try to sign in silently with the account that the user is signed into Windows
            else
            {
                result = await app.AcquireTokenSilent(acquireScopes, PublicClientApplication.OperatingSystemAccount)
                    .ExecuteAsync();
            }
        }
        // Can't get a token silently, go interactive
        catch (MsalUiRequiredException ex)
        {
            try
            {
                result = await app.AcquireTokenInteractive(acquireScopes).ExecuteAsync();
            }
            catch (Exception iex)
            {
                Log.Error("Interactive authentication failed: {Error}", iex.Message);
                StatusTextBlock.Text = "Authentication failed";
                return null;
            }
        }
        catch (Exception ex)
        {
            Log.Error("Authentication failed: {Error}", ex.Message);
            StatusTextBlock.Text = "Authentication failed";
            return null;
        }

        return result;
    }

    private async Task<AuthenticationResult?> ExchangeTokenWithKeycloak(AuthenticationResult entraIdResult)
    {
        var subjectToken = entraIdResult.AccessToken;

        if (string.IsNullOrEmpty(subjectToken))
        {
            Log.Error("No EntraId access token available for exchange");
            StatusTextBlock.Text = "No EntraId token";
            return null;
        }
        var keycloakEndpoint = Environment.GetEnvironmentVariable("KEYCLOAK_TOKEN_ENDPOINT")
                               ?? "http://localhost:8080/realms/sso-probe/protocol/openid-connect/token";
        var clientId = Environment.GetEnvironmentVariable("KEYCLOAK_CLIENT_ID");
        var clientSecret = Environment.GetEnvironmentVariable("KEYCLOAK_CLIENT_SECRET");

        if (string.IsNullOrEmpty(clientId))
        {
            Log.Error("Keycloak client id not configured (env KEYCLOAK_CLIENT_ID)");
            StatusTextBlock.Text = "Keycloak client id not configured";
            return null;
        }

        if (string.IsNullOrEmpty(clientSecret))
        {
            Log.Error("Keycloak client secret not configured (env KEYCLOAK_CLIENT_SECRET)");
            StatusTextBlock.Text = "Keycloak client secret not configured";
            return null;
        }

        try
        {
            using var http = new System.Net.Http.HttpClient();
            Log.Information("Exchanging token with Keycloak at {Endpoint} for client {ClientId}", keycloakEndpoint, clientId);
            var form = new List<KeyValuePair<string, string>>
            {
                new("grant_type", "urn:ietf:params:oauth:grant-type:token-exchange"),
                new("subject_token", subjectToken),
                // new("subject_token_type", "urn:ietf:params:oauth:token-type:id_token"),
                new("subject_token_type", "urn:ietf:params:oauth:token-type:access_token"),
                new("subject_issues", "entraid-saml"),
                // new("requested_token_type", "urn:ietf:params:oauth:token-type:id_token"),
                new("client_id", clientId),
                new("client_secret", clientSecret),
                new("scope", "openid"),
            };

            var content = new System.Net.Http.FormUrlEncodedContent(form);
            Log.Debug("Posting token-exchange to Keycloak endpoint {Endpoint}", keycloakEndpoint);
            var resp = await http.PostAsync(keycloakEndpoint, content);
            var body = await resp.Content.ReadAsStringAsync();

            if (!resp.IsSuccessStatusCode)
            {
                Log.Error("Keycloak token exchange failed ({Status}): {Body}", (int)resp.StatusCode, body);
                StatusTextBlock.Text = "Keycloak token exchange failed";
                return null;
            }

            using var doc = System.Text.Json.JsonDocument.Parse(body);
            var root = doc.RootElement;

            string? kcAccess =
                root.TryGetProperty("access_token", out var at) && at.ValueKind == System.Text.Json.JsonValueKind.String
                    ? at.GetString()
                    : null;
            string? kcIdToken =
                root.TryGetProperty("id_token", out var it) && it.ValueKind == System.Text.Json.JsonValueKind.String
                    ? it.GetString()
                    : null;
            int expiresIn = 0;
            if (root.TryGetProperty("expires_in", out var ei))
            {
                if (ei.ValueKind == System.Text.Json.JsonValueKind.Number && ei.TryGetInt32(out var val))
                {
                    expiresIn = val;
                }
                else
                {
                    int.TryParse(ei.GetString(), out expiresIn);
                }
            }

            string? scope =
                root.TryGetProperty("scope", out var sc) && sc.ValueKind == System.Text.Json.JsonValueKind.String
                    ? sc.GetString()
                    : null;

            Log.Information(
                "Keycloak exchange succeeded. AccessTokenLength: {Len}, HasIdToken: {HasId}, ExpiresIn: {Expires}, Scope: {Scope}",
                kcAccess?.Length ?? 0, !string.IsNullOrEmpty(kcIdToken), expiresIn, scope ?? "(none)");

            StatusTextBlock.Text = "Keycloak token exchange succeeded";

            // Note: MSAL's AuthenticationResult cannot be constructed here; return null after performing the exchange.
            return null;
        }
        catch (Exception ex)
        {
            Log.Error("Exception during Keycloak token exchange: {Error}", ex.Message);
            StatusTextBlock.Text = "Token exchange error";
            return null;
        }
    }
}