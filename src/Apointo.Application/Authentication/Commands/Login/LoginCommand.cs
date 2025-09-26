using Apointo.Application.Authentication.Common;
using Apointo.Application.Common.Interfaces.Identity;
using Apointo.Application.Common.Models;
using MediatR;

namespace Apointo.Application.Authentication.Commands.Login;

public sealed record LoginCommand(
    string Email,
    string Password,
    string? Device,
    string? IpAddress) : IRequest<Result<AuthenticationResult>>;

public sealed class LoginCommandHandler : IRequestHandler<LoginCommand, Result<AuthenticationResult>>
{
    private readonly IIdentityService _identityService;

    public LoginCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<Result<AuthenticationResult>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var loginRequest = new LoginIdentityRequest(
            request.Email,
            request.Password,
            request.Device,
            request.IpAddress);

        return await _identityService.LoginAsync(loginRequest, cancellationToken);
    }
}