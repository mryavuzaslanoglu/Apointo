using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Apointo.Application.Authentication.Common;
using Apointo.Application.Common.Interfaces.Authentication;
using Apointo.Application.Configuration;
using Apointo.Application.Common.Interfaces;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Apointo.Infrastructure.Services;

public sealed class TokenService : ITokenService
{
    private readonly JwtSettings _jwtSettings;
    private readonly IDateTimeProvider _dateTimeProvider;

    public TokenService(IOptions<JwtSettings> jwtOptions, IDateTimeProvider dateTimeProvider)
    {
        _jwtSettings = jwtOptions.Value;
        _dateTimeProvider = dateTimeProvider;
    }

    public TokenPair GenerateTokenPair(TokenGenerationRequest request)
    {
        var now = _dateTimeProvider.UtcNow;
        var accessTokenExpiresAt = now.AddMinutes(_jwtSettings.AccessTokenLifetimeMinutes);
        var refreshTokenExpiresAt = now.AddDays(_jwtSettings.RefreshTokenLifetimeDays);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, request.UserId.ToString()),
            new(JwtRegisteredClaimNames.Email, request.Email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new("fullName", request.FullName)
        };

        foreach (var role in request.Roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var signingCredentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SigningKey)),
            SecurityAlgorithms.HmacSha256);

        var jwtToken = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            notBefore: now,
            expires: accessTokenExpiresAt,
            signingCredentials: signingCredentials);

        var accessToken = new JwtSecurityTokenHandler().WriteToken(jwtToken);

        var refreshToken = GenerateSecureRandomToken();
        var salt = GenerateSalt();
        var hash = ComputeRefreshTokenHash(refreshToken, salt);

        return new TokenPair(
            accessToken,
            accessTokenExpiresAt,
            refreshToken,
            refreshTokenExpiresAt,
            hash,
            salt);
    }

    public string ComputeRefreshTokenHash(string refreshToken, string salt)
    {
        var value = Encoding.UTF8.GetBytes(refreshToken + salt);
        var hashBytes = SHA256.HashData(value);
        return Convert.ToBase64String(hashBytes);
    }

    private static string GenerateSecureRandomToken()
    {
        var bytes = new byte[64];
        RandomNumberGenerator.Fill(bytes);
        return Convert.ToBase64String(bytes);
    }

    private static string GenerateSalt()
    {
        var bytes = new byte[32];
        RandomNumberGenerator.Fill(bytes);
        return Convert.ToBase64String(bytes);
    }
}