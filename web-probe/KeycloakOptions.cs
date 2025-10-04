namespace dotnet_sso_web_probe;

public class KeycloakOptions
{
    public string Url { get; set; } = string.Empty;
    public string Realm { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
}