using Apointo.Application.Common.Interfaces.Identity;
using Apointo.Application.Common.Models;
using MediatR;

namespace Apointo.Application.Authentication.Commands.Logout;

public sealed record LogoutCommand(
    Guid UserId,
    string RefreshToken) : IRequest<Result>;

public sealed class LogoutCommandHandler : IRequestHandler<LogoutCommand, Result>
{
    private readonly IIdentityService _identityService;

    public LogoutCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public async Task<Result> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        var revokeRequest = new RevokeRefreshIdentityRequest(request.UserId, request.RefreshToken);
        return await _identityService.RevokeRefreshTokenAsync(revokeRequest, cancellationToken);
    }
}