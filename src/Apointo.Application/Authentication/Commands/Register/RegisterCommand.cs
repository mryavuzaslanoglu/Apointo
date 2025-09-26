using Apointo.Application.Authentication.Common;
using Apointo.Application.Common.Interfaces.Identity;
using Apointo.Application.Common.Models;
using MediatR;

namespace Apointo.Application.Authentication.Commands.Register;

public sealed record RegisterCommand(
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string? PhoneNumber,
    string Role,
    string? Device,
    string? IpAddress) : IRequest<Result<AuthenticationResult>>;

public sealed class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result<AuthenticationResult>>
{
    private readonly IIdentityService _identityService;

    public RegisterCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<Result<AuthenticationResult>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var registerRequest = new RegisterIdentityRequest(
            request.Email,
            request.Password,
            request.FirstName,
            request.LastName,
            request.PhoneNumber,
            request.Role,
            request.Device,
            request.IpAddress);

        return await _identityService.RegisterAsync(registerRequest, cancellationToken);
    }
}