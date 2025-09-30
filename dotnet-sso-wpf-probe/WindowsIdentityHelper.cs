using System.Security.Principal;
using System.Text;

namespace dotnet_sso_wpf_probe;

public static class WindowsIdentityHelper
{
    public static string GetWindowsIdentityInfo()
    {
        var identity = WindowsIdentity.GetCurrent();
        var sb = new StringBuilder();
        sb.AppendLine($"Name: {identity.Name}");
        sb.AppendLine($"AuthType: {identity.AuthenticationType}");
        sb.AppendLine($"IsAuthenticated: {identity.IsAuthenticated}");
        sb.AppendLine($"IsAnonymous: {identity.IsAnonymous}");
        sb.AppendLine($"IsGuest: {identity.IsGuest}");
        sb.AppendLine($"IsSystem: {identity.IsSystem}");
        sb.AppendLine($"Token: {identity.Token}");
        // Groups
        var groups = identity.Groups;
        if (groups != null)
        {
            sb.AppendLine("Groups:");
            foreach (var group in groups)
            {
                try
                {
                    var sid = group.Translate(typeof(System.Security.Principal.NTAccount));
                    sb.AppendLine($"  - {sid.Value}");
                }
                catch
                {
                    sb.AppendLine($"  - {group.Value}");
                }
            }
        }
        // Claims (if available)
        if (identity is System.Security.Claims.ClaimsIdentity ci)
        {
            sb.AppendLine("Claims:");
            foreach (var claim in ci.Claims)
            {
                sb.AppendLine($"  - {claim.Type}: {claim.Value}");
            }
        }

        return sb.ToString();
    }

}