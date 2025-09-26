using Apointo.Application.Authentication.Common;
using Apointo.Application.Common.Models;

namespace Apointo.Application.Common.Interfaces.Identity;

public interface IIdentityService
{
    Task<Result<AuthenticationResult>> RegisterAsync(RegisterIdentityRequest request, CancellationToken cancellationToken);
    Task<Result<AuthenticationResult>> LoginAsync(LoginIdentityRequest request, CancellationToken cancellationToken);
    Task<Result<AuthenticationResult>> RefreshTokenAsync(RefreshIdentityRequest request, CancellationToken cancellationToken);
    Task<Result> RevokeRefreshTokenAsync(RevokeRefreshIdentityRequest request, CancellationToken cancellationToken);
    Task<Result> ForgotPasswordAsync(ForgotPasswordIdentityRequest request, CancellationToken cancellationToken);
    Task<Result> ResetPasswordAsync(ResetPasswordIdentityRequest request, CancellationToken cancellationToken);
}

public sealed record RegisterIdentityRequest(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string? PhoneNumber,
    string Role,
    string? Device,
    string? IpAddress);

public sealed record LoginIdentityRequest(
    string Email,
    string Password,
    string? Device,
    string? IpAddress);

public sealed record RefreshIdentityRequest(
    Guid UserId,
    string RefreshToken,
    string? Device,
    string? IpAddress);

public sealed record RevokeRefreshIdentityRequest(
    Guid UserId,
    string RefreshToken);

public sealed record ForgotPasswordIdentityRequest(
    string Email,
    string? ClientBaseUrl);

public sealed record ResetPasswordIdentityRequest(
    Guid UserId,
    string Token,
    string NewPassword,
    string ConfirmPassword);