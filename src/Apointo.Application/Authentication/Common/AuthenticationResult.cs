namespace Apointo.Application.Authentication.Common;

public sealed record AuthenticationResult(
    Guid UserId,
    string Email,
    string FullName,
    string AccessToken,
    DateTime AccessTokenExpiresAtUtc,
    string RefreshToken,
    DateTime RefreshTokenExpiresAtUtc,
    IReadOnlyCollection<string> Roles);