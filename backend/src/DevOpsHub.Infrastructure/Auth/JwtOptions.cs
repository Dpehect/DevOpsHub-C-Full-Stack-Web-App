namespace DevOpsHub.Infrastructure.Auth;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";
    public string Issuer { get; init; } = "DevOpsHub";
    public string Audience { get; init; } = "DevOpsHub.Web";
    public string SigningKey { get; init; } = string.Empty;
    public int AccessTokenMinutes { get; init; } = 30;
    public int RefreshTokenDays { get; init; } = 14;
}
