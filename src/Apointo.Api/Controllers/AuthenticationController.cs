using Apointo.Api.Contracts.Authentication;
using Apointo.Application.Authentication.Commands.ForgotPassword;
using Apointo.Application.Authentication.Commands.Login;
using Apointo.Application.Authentication.Commands.Logout;
using Apointo.Application.Authentication.Commands.RefreshToken;
using Apointo.Application.Authentication.Commands.Register;
using Apointo.Application.Authentication.Commands.ResetPassword;
using Apointo.Application.Authentication.Common;
using Apointo.Application.Common.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Apointo.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthenticationController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthenticationController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthenticationResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<AuthenticationResponse>> Register(
        [FromBody] RegisterRequest request,
        CancellationToken cancellationToken)
    {
        var command = new RegisterCommand(
            request.Email,
            request.Password,
            request.FirstName,
            request.LastName,
            request.PhoneNumber,
            request.Role,
            request.Device,
            request.IpAddress);

        var result = await _mediator.Send(command, cancellationToken);
        return result.IsSuccess
            ? Ok(ToResponse(result.Value!))
            : HandleFailure(result.ToNonGeneric());
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthenticationResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<AuthenticationResponse>> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        var command = new LoginCommand(
            request.Email,
            request.Password,
            request.Device,
            request.IpAddress);

        var result = await _mediator.Send(command, cancellationToken);
        return result.IsSuccess
            ? Ok(ToResponse(result.Value!))
            : HandleFailure(result.ToNonGeneric());
    }

    [HttpPost("refresh")]
    [ProducesResponseType(typeof(AuthenticationResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<AuthenticationResponse>> RefreshToken(
        [FromBody] RefreshRequest request,
        CancellationToken cancellationToken)
    {
        var command = new RefreshTokenCommand(
            request.UserId,
            request.RefreshToken,
            request.Device,
            request.IpAddress);

        var result = await _mediator.Send(command, cancellationToken);
        return result.IsSuccess
            ? Ok(ToResponse(result.Value!))
            : HandleFailure(result.ToNonGeneric());
    }

    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Logout(
        [FromBody] LogoutRequest request,
        CancellationToken cancellationToken)
    {
        var command = new LogoutCommand(request.UserId, request.RefreshToken);
        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsSuccess)
        {
            return NoContent();
        }

        return HandleFailure(result);
    }

    [HttpPost("forgot-password")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    public async Task<IActionResult> ForgotPassword(
        [FromBody] ForgotPasswordRequest request,
        CancellationToken cancellationToken)
    {
        var command = new ForgotPasswordCommand(request.Email, request.ClientBaseUrl);
        var result = await _mediator.Send(command, cancellationToken);

        if (result.IsSuccess)
        {
            return Accepted();
        }

        return HandleFailure(result);
    }

    [HttpPost("reset-password")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ResetPassword(
        [FromBody] ResetPasswordRequest request,
        CancellationToken cancellationToken)
    {
        var command = new ResetPasswordCommand(
            request.UserId,
            request.Token,
            request.NewPassword,
            request.ConfirmPassword);

        var result = await _mediator.Send(command, cancellationToken);
        if (result.IsSuccess)
        {
            return NoContent();
        }

        return HandleFailure(result);
    }

    private static AuthenticationResponse ToResponse(AuthenticationResult result)
    {
        return new AuthenticationResponse(
            result.UserId,
            result.Email,
            result.FullName,
            result.AccessToken,
            result.AccessTokenExpiresAtUtc,
            result.RefreshToken,
            result.RefreshTokenExpiresAtUtc,
            result.Roles);
    }

    private ActionResult HandleFailure(Result result)
    {
        var errorCode = result.Error;
        var detail = GetErrorMessage(errorCode);

        return errorCode switch
        {
            "EmailAlreadyExists" => Conflict(CreateProblemDetails(errorCode, detail, StatusCodes.Status409Conflict)),
            "RoleNotFound" => BadRequest(CreateProblemDetails(errorCode, detail, StatusCodes.Status400BadRequest)),
            "InvalidCredentials" => Unauthorized(CreateProblemDetails(errorCode, detail, StatusCodes.Status401Unauthorized)),
            "UserInactive" => StatusCode(StatusCodes.Status403Forbidden, CreateProblemDetails(errorCode, detail, StatusCodes.Status403Forbidden)),
            "UserLockedOut" => StatusCode(StatusCodes.Status423Locked, CreateProblemDetails(errorCode, detail, StatusCodes.Status423Locked)),
            "InvalidRefreshToken" => Unauthorized(CreateProblemDetails(errorCode, detail, StatusCodes.Status401Unauthorized)),
            "UserNotFound" => NotFound(CreateProblemDetails(errorCode, detail, StatusCodes.Status404NotFound)),
            "EmailDisabled" => StatusCode(StatusCodes.Status503ServiceUnavailable, CreateProblemDetails(errorCode, detail, StatusCodes.Status503ServiceUnavailable)),
            "PasswordResetUrlNotConfigured" => StatusCode(StatusCodes.Status500InternalServerError, CreateProblemDetails(errorCode, detail, StatusCodes.Status500InternalServerError)),
            "PasswordsDoNotMatch" => BadRequest(CreateProblemDetails(errorCode, detail, StatusCodes.Status400BadRequest)),
            null => Problem(detail: detail),
            _ => Problem(detail: detail)
        };
    }

    private static string GetErrorMessage(string? errorCode)
    {
        return errorCode switch
        {
            "EmailAlreadyExists" => "Bu e-posta adresi zaten kullanılıyor.",
            "RoleNotFound" => "Talep edilen rol bulunamadı.",
            "InvalidCredentials" => "E-posta veya şifre hatalı.",
            "UserInactive" => "Kullanıcı hesabı aktif değil.",
            "UserLockedOut" => "Kullanıcı hesabı geçici olarak kilitlendi.",
            "InvalidRefreshToken" => "Geçersiz oturum yenileme isteği.",
            "UserNotFound" => "Kullanıcı bulunamadı.",
            "EmailDisabled" => "E-posta servisi şu anda kullanılamıyor.",
            "PasswordResetUrlNotConfigured" => "Şifre sıfırlama bağlantısı yapılandırılmamış. Lütfen sistem yöneticinizle iletişime geçin.",
            "PasswordsDoNotMatch" => "Şifreler birbiriyle uyuşmuyor.",
            _ => "Beklenmeyen bir hata oluştu."
        };
    }

    private static ProblemDetails CreateProblemDetails(string? errorCode, string detail, int statusCode)
    {
        var problemDetails = new ProblemDetails
        {
            Title = detail,
            Detail = detail,
            Status = statusCode
        };

        if (!string.IsNullOrWhiteSpace(errorCode))
        {
            problemDetails.Extensions["code"] = errorCode;
        }

        return problemDetails;
    }
}
