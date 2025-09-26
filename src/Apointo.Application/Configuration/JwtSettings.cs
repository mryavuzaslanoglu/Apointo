namespace Apointo.Application.Configuration;

public sealed class JwtSettings
{
    public const string SectionName = "Jwt";

    public string Issuer { get; init; } = string.Empty;
    public string Audience { get; init; } = string.Empty;
    public int AccessTokenLifetimeMinutes { get; init; }
    public int RefreshTokenLifetimeDays { get; init; }
    public string SigningKey { get; init; } = string.Empty;
}