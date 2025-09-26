using System;
using System.Threading;
using System.Threading.Tasks;
using Apointo.Application.Common.Interfaces.Persistence;
using Apointo.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Apointo.Application.ServiceCatalog.Commands.DeleteService;

public sealed record DeleteServiceCommand(Guid ServiceId) : IRequest<Result>;

public sealed class DeleteServiceCommandHandler : IRequestHandler<DeleteServiceCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public DeleteServiceCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(DeleteServiceCommand request, CancellationToken cancellationToken)
    {
        var service = await _context.Services
            .FirstOrDefaultAsync(s => s.Id == request.ServiceId, cancellationToken);

        if (service is null)
        {
            return Result.Failure("ServiceNotFound");
        }

        _context.Services.Remove(service);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
