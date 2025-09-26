namespace Apointo.Api.Contracts.Authentication;

public sealed record RegisterRequest(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string? PhoneNumber,
    string Role,
    string? Device,
    string? IpAddress);

public sealed record LoginRequest(
    string Email,
    string Password,
    string? Device,
    string? IpAddress);

public sealed record RefreshRequest(
    Guid UserId,
    string RefreshToken,
    string? Device,
    string? IpAddress);

public sealed record LogoutRequest(
    Guid UserId,
    string RefreshToken);

public sealed record ForgotPasswordRequest(
    string Email,
    string? ClientBaseUrl);

public sealed record ResetPasswordRequest(
    Guid UserId,
    string Token,
    string NewPassword,
    string ConfirmPassword);

public sealed record AuthenticationResponse(
    Guid UserId,
    string Email,
    string FullName,
    string AccessToken,
    DateTime AccessTokenExpiresAtUtc,
    string RefreshToken,
    DateTime RefreshTokenExpiresAtUtc,
    IReadOnlyCollection<string> Roles);