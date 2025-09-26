using Apointo.Application.Common.Interfaces.Identity;
using Apointo.Application.Common.Models;
using MediatR;

namespace Apointo.Application.Authentication.Commands.ResetPassword;

public sealed record ResetPasswordCommand(Guid UserId, string Token, string NewPassword, string ConfirmPassword) : IRequest<Result>;

public sealed class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, Result>
{
    private readonly IIdentityService _identityService;

    public ResetPasswordCommandHandler(IIdentityService identityService)
    {
        _identityService = identityService;
    }

    public Task<Result> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var identityRequest = new ResetPasswordIdentityRequest(
            request.UserId,
            request.Token,
            request.NewPassword,
            request.ConfirmPassword);

        return _identityService.ResetPasswordAsync(identityRequest, cancellationToken);
    }
}