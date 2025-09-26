using System;
using System.Threading;
using System.Threading.Tasks;
using Apointo.Application.Common.Interfaces.Persistence;
using Apointo.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Apointo.Application.Staff.Commands.DeleteAvailabilityOverride;

public sealed record DeleteStaffAvailabilityOverrideCommand(Guid StaffId, Guid OverrideId) : IRequest<Result>;

public sealed class DeleteStaffAvailabilityOverrideCommandHandler : IRequestHandler<DeleteStaffAvailabilityOverrideCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public DeleteStaffAvailabilityOverrideCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(DeleteStaffAvailabilityOverrideCommand request, CancellationToken cancellationToken)
    {
        var overrideEntity = await _context.StaffAvailabilityOverrides
            .FirstOrDefaultAsync(o => o.Id == request.OverrideId && o.StaffId == request.StaffId, cancellationToken);

        if (overrideEntity is null)
        {
            return Result.Failure("AvailabilityOverrideNotFound");
        }

        _context.StaffAvailabilityOverrides.Remove(overrideEntity);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
