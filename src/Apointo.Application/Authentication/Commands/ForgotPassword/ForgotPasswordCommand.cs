using Apointo.Application.Common.Interfaces.Identity;
using Apointo.Application.Common.Models;
using MediatR;

namespace Apointo.Application.Authentication.Commands.ForgotPassword;

public sealed record ForgotPasswordCommand(string Email, string? ClientBaseUrl) : IRequest<Result>;

public sealed class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, Result>
{
    private readonly IIdentityService _identityService;

    public ForgotPasswordCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public Task<Result> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var identityRequest = new ForgotPasswordIdentityRequest(request.Email, request.ClientBaseUrl);
        return _identityService.ForgotPasswordAsync(identityRequest, cancellationToken);
    }
}