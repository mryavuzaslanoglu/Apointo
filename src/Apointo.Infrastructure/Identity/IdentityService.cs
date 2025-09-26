using System;
using System.Linq;
using Apointo.Application.Authentication.Common;
using Apointo.Application.Common.Interfaces;
using Apointo.Application.Common.Interfaces.Authentication;
using Apointo.Application.Common.Interfaces.Identity;
using Apointo.Application.Common.Interfaces.Notifications;
using Apointo.Application.Common.Models;
using Apointo.Application.Configuration;
using Apointo.Domain.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Apointo.Infrastructure.Identity;

public sealed class IdentityService : IIdentityService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly ITokenService _tokenService;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IEmailSender _emailSender;
    private readonly EmailSettings _emailSettings;
    private readonly ILogger<IdentityService> _logger;

    public IdentityService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        RoleManager<ApplicationRole> roleManager,
        ITokenService tokenService,
        IRefreshTokenRepository refreshTokenRepository,
        IDateTimeProvider dateTimeProvider,
        IEmailSender emailSender,
        IOptions<EmailSettings> emailOptions,
        ILogger<IdentityService> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
        _tokenService = tokenService;
        _refreshTokenRepository = refreshTokenRepository;
        _dateTimeProvider = dateTimeProvider;
        _emailSender = emailSender;
        _emailSettings = emailOptions.Value;
        _logger = logger;
    }

    public async Task<Result<AuthenticationResult>> RegisterAsync(RegisterIdentityRequest request, CancellationToken cancellationToken)
    {
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser is not null)
        {
            return Result<AuthenticationResult>.Failure("EmailAlreadyExists");
        }

        if (!await _roleManager.RoleExistsAsync(request.Role))
        {
            return Result<AuthenticationResult>.Failure("RoleNotFound");
        }

        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            PhoneNumber = request.PhoneNumber,
            EmailConfirmed = true,
            IsActive = true
        };

        var identityResult = await _userManager.CreateAsync(user, request.Password);
        if (!identityResult.Succeeded)
        {
            return Result<AuthenticationResult>.Failure(string.Join(";", identityResult.Errors.Select(e => e.Description)));
        }

        var addToRoleResult = await _userManager.AddToRoleAsync(user, request.Role);
        if (!addToRoleResult.Succeeded)
        {
            await _userManager.DeleteAsync(user);
            return Result<AuthenticationResult>.Failure(string.Join(";", addToRoleResult.Errors.Select(e => e.Description)));
        }

        var roles = await _userManager.GetRolesAsync(user);
        var roleArray = roles.ToArray();
        var now = _dateTimeProvider.UtcNow;

        await _refreshTokenRepository.RemoveExpiredTokensAsync(user.Id, now, cancellationToken);

        var tokenPair = _tokenService.GenerateTokenPair(new TokenGenerationRequest(
            user.Id,
            user.Email!,
            BuildFullName(user),
            roleArray));

        var refreshToken = RefreshToken.Create(
            user.Id,
            tokenPair.RefreshTokenHash,
            tokenPair.RefreshTokenSalt,
            now,
            tokenPair.RefreshTokenExpiresAtUtc,
            request.Device,
            request.IpAddress);
        refreshToken.CreatedBy = user.Id;

        await _refreshTokenRepository.AddAsync(refreshToken, cancellationToken);
        await _refreshTokenRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("New user registered with email {Email} and role {Role}", request.Email, request.Role);

        return Result<AuthenticationResult>.Success(new AuthenticationResult(
            user.Id,
            user.Email!,
            BuildFullName(user),
            tokenPair.AccessToken,
            tokenPair.AccessTokenExpiresAtUtc,
            tokenPair.RefreshToken,
            tokenPair.RefreshTokenExpiresAtUtc,
            roleArray));
    }

    public async Task<Result<AuthenticationResult>> LoginAsync(LoginIdentityRequest request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null)
        {
            _logger.LogWarning("Login attempt failed for email {Email}: user not found", request.Email);
            return Result<AuthenticationResult>.Failure("InvalidCredentials");
        }

        if (!user.IsActive)
        {
            _logger.LogWarning("Login attempt blocked because user {UserId} is inactive", user.Id);
            return Result<AuthenticationResult>.Failure("UserInactive");
        }

        var signInResult = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
        if (!signInResult.Succeeded)
        {
            var failureCode = signInResult.IsLockedOut ? "UserLockedOut" : "InvalidCredentials";
            var failureReason = signInResult.IsLockedOut ? "account locked" : "wrong credentials";
            _logger.LogWarning(
                "Login attempt failed for user {UserId} ({Email}): {Reason}",
                user.Id,
                request.Email,
                failureReason);
            return Result<AuthenticationResult>.Failure(failureCode);
        }

        var roles = await _userManager.GetRolesAsync(user);
        if (roles.Count == 0)
        {
            var addRoleResult = await _userManager.AddToRoleAsync(user, RoleNames.Customer);
            if (!addRoleResult.Succeeded)
            {
                var errorMessage = string.Join(";", addRoleResult.Errors.Select(e => e.Description));
                _logger.LogError(
                    "Role assignment failed for user {UserId} during login: {Errors}",
                    user.Id,
                    errorMessage);
                return Result<AuthenticationResult>.Failure(errorMessage);
            }

            roles = await _userManager.GetRolesAsync(user);
        }

        var roleArray = roles.ToArray();
        var now = _dateTimeProvider.UtcNow;

        await _refreshTokenRepository.RemoveExpiredTokensAsync(user.Id, now, cancellationToken);

        var tokenPair = _tokenService.GenerateTokenPair(new TokenGenerationRequest(
            user.Id,
            user.Email!,
            BuildFullName(user),
            roleArray));

        var refreshToken = RefreshToken.Create(
            user.Id,
            tokenPair.RefreshTokenHash,
            tokenPair.RefreshTokenSalt,
            now,
            tokenPair.RefreshTokenExpiresAtUtc,
            request.Device,
            request.IpAddress);
        refreshToken.CreatedBy = user.Id;

        await _refreshTokenRepository.AddAsync(refreshToken, cancellationToken);
        await _refreshTokenRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "User {UserId} logged in from IP {IpAddress} using device {Device}",
            user.Id,
            string.IsNullOrWhiteSpace(request.IpAddress) ? "unknown" : request.IpAddress,
            string.IsNullOrWhiteSpace(request.Device) ? "unknown" : request.Device);

        return Result<AuthenticationResult>.Success(new AuthenticationResult(
            user.Id,
            user.Email!,
            BuildFullName(user),
            tokenPair.AccessToken,
            tokenPair.AccessTokenExpiresAtUtc,
            tokenPair.RefreshToken,
            tokenPair.RefreshTokenExpiresAtUtc,
            roleArray));
    }

    public async Task<Result<AuthenticationResult>> RefreshTokenAsync(RefreshIdentityRequest request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.UserId.ToString());
        if (user is null || !user.IsActive)
        {
            return Result<AuthenticationResult>.Failure("UserNotFound");
        }

        var now = _dateTimeProvider.UtcNow;

        await _refreshTokenRepository.RemoveExpiredTokensAsync(user.Id, now, cancellationToken);

        var activeTokens = await _refreshTokenRepository.GetActiveTokensByUserIdAsync(user.Id, cancellationToken);
        var matchingToken = activeTokens.FirstOrDefault(token =>
            token.IsActive(now) &&
            token.TokenHash == _tokenService.ComputeRefreshTokenHash(request.RefreshToken, token.TokenSalt));

        if (matchingToken is null)
        {
            return Result<AuthenticationResult>.Failure("InvalidRefreshToken");
        }

        var roles = await _userManager.GetRolesAsync(user);
        var roleArray = roles.ToArray();

        var tokenPair = _tokenService.GenerateTokenPair(new TokenGenerationRequest(
            user.Id,
            user.Email!,
            BuildFullName(user),
            roleArray));

        matchingToken.Revoke(now, tokenPair.RefreshTokenHash);
        matchingToken.LastModifiedBy = user.Id;

        var newRefreshToken = RefreshToken.Create(
            user.Id,
            tokenPair.RefreshTokenHash,
            tokenPair.RefreshTokenSalt,
            now,
            tokenPair.RefreshTokenExpiresAtUtc,
            request.Device,
            request.IpAddress);
        newRefreshToken.CreatedBy = user.Id;

        await _refreshTokenRepository.AddAsync(newRefreshToken, cancellationToken);
        await _refreshTokenRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Refresh token rotated for user {UserId}", user.Id);

        return Result<AuthenticationResult>.Success(new AuthenticationResult(
            user.Id,
            user.Email!,
            BuildFullName(user),
            tokenPair.AccessToken,
            tokenPair.AccessTokenExpiresAtUtc,
            tokenPair.RefreshToken,
            tokenPair.RefreshTokenExpiresAtUtc,
            roleArray));
    }

    public async Task<Result> RevokeRefreshTokenAsync(RevokeRefreshIdentityRequest request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.UserId.ToString());
        if (user is null)
        {
            return Result.Failure("UserNotFound");
        }

        var now = _dateTimeProvider.UtcNow;
        await _refreshTokenRepository.RemoveExpiredTokensAsync(user.Id, now, cancellationToken);

        var activeTokens = await _refreshTokenRepository.GetActiveTokensByUserIdAsync(user.Id, cancellationToken);
        var matchingToken = activeTokens.FirstOrDefault(token =>
            token.TokenHash == _tokenService.ComputeRefreshTokenHash(request.RefreshToken, token.TokenSalt));

        if (matchingToken is null)
        {
            return Result.Failure("InvalidRefreshToken");
        }

        matchingToken.Revoke(now);
        matchingToken.LastModifiedBy = user.Id;
        await _refreshTokenRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Refresh token revoked for user {UserId}", user.Id);

        return Result.Success();
    }

    public async Task<Result> ForgotPasswordAsync(ForgotPasswordIdentityRequest request, CancellationToken cancellationToken)
    {
        if (!_emailSettings.IsEnabled)
        {
            _logger.LogWarning("Email sending is disabled. Cannot process forgot password for {Email}", request.Email);
            return Result.Failure("EmailDisabled");
        }

        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user is null || !await _userManager.IsEmailConfirmedAsync(user))
        {
            _logger.LogInformation("Password reset requested for non-existing or unconfirmed email {Email}", request.Email);
            return Result.Success();
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var encodedToken = Uri.EscapeDataString(token);
        var baseUrl = !string.IsNullOrWhiteSpace(request.ClientBaseUrl)
            ? request.ClientBaseUrl!
            : _emailSettings.PasswordResetUrl;

        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            _logger.LogWarning("Password reset base URL is not configured. Cannot send reset link for user {UserId}", user.Id);
            return Result.Failure("PasswordResetUrlNotConfigured");
        }

        var resetLink = $"{baseUrl.TrimEnd('/')}/reset-password?userId={user.Id}&token={encodedToken}";
        var subject = "Apointo Password Reset";
        var body = $"""
            <p>Merhaba {BuildFullName(user)},</p>
            <p>Şifrenizi sıfırlamak için aşağıdaki bağlantıya tıklayın:</p>
            <p><a href="{resetLink}">Şifreyi sıfırla</a></p>
            <p>Eğer bu işlemi siz başlatmadıysanız lütfen bu e-postayı yok sayın.</p>
        """;

        await _emailSender.SendAsync(user.Email!, subject, body, cancellationToken);
        _logger.LogInformation("Password reset email sent to user {UserId}", user.Id);

        return Result.Success();
    }

    public async Task<Result> ResetPasswordAsync(ResetPasswordIdentityRequest request, CancellationToken cancellationToken)
    {
        if (!string.Equals(request.NewPassword, request.ConfirmPassword, StringComparison.Ordinal))
        {
            return Result.Failure("PasswordsDoNotMatch");
        }

        var user = await _userManager.FindByIdAsync(request.UserId.ToString());
        if (user is null)
        {
            return Result.Failure("UserNotFound");
        }

        var decodedToken = Uri.UnescapeDataString(request.Token);
        var result = await _userManager.ResetPasswordAsync(user, decodedToken, request.NewPassword);
        if (!result.Succeeded)
        {
            var errorMessage = string.Join(";", result.Errors.Select(e => e.Description));
            _logger.LogWarning("Password reset failed for user {UserId}: {Errors}", user.Id, errorMessage);
            return Result.Failure(errorMessage);
        }

        _logger.LogInformation("Password reset successful for user {UserId}", user.Id);
        return Result.Success();
    }

    private static string BuildFullName(ApplicationUser user)
    {
        return string.Join(" ", new[] { user.FirstName, user.LastName }.Where(x => !string.IsNullOrWhiteSpace(x))).Trim();
    }
}
