namespace TaskManagement.Infrastructure.Security;

/// <summary>
/// Bound from the "Jwt" configuration section. The signing Key is a secret and
/// must come from user-secrets / environment variables, never appsettings.json.
/// </summary>
public class JwtSettings
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
    public int ExpiryMinutes { get; set; } = 60;
}
