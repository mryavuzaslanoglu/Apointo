using Apointo.Application.Authentication.Common;
using Apointo.Application.Common.Interfaces.Identity;
using Apointo.Application.Common.Models;
using MediatR;

namespace Apointo.Application.Authentication.Commands.RefreshToken;

public sealed record RefreshTokenCommand(
    Guid UserId,
    string RefreshToken,
    string? Device,
    string? IpAddress) : IRequest<Result<AuthenticationResult>>;

public sealed class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, Result<AuthenticationResult>>
{
    private readonly IIdentityService _identityService;

    public RefreshTokenCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<Result<AuthenticationResult>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var refreshRequest = new RefreshIdentityRequest(
            request.UserId,
            request.RefreshToken,
            request.Device,
            request.IpAddress);

        return await _identityService.RefreshTokenAsync(refreshRequest, cancellationToken);
    }
}