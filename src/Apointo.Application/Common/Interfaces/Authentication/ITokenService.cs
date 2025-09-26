using Apointo.Application.Authentication.Common;

namespace Apointo.Application.Common.Interfaces.Authentication;

public interface ITokenService
{
    TokenPair GenerateTokenPair(TokenGenerationRequest request);
    string ComputeRefreshTokenHash(string refreshToken, string salt);
}

public sealed record TokenGenerationRequest(
    Guid UserId,
    string Email,
    string FullName,
    IReadOnlyCollection<string> Roles);

public sealed record TokenPair(
    string AccessToken,
    DateTime AccessTokenExpiresAtUtc,
    string RefreshToken,
    DateTime RefreshTokenExpiresAtUtc,
    string RefreshTokenHash,
    string RefreshTokenSalt);