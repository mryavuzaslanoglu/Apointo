using System;
using System.Threading;
using System.Threading.Tasks;
using Apointo.Application.Common.Interfaces.Persistence;
using Apointo.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Apointo.Application.Staff.Commands.DeleteStaff;

public sealed record DeleteStaffCommand(Guid StaffId) : IRequest<Result>;

public sealed class DeleteStaffCommandHandler : IRequestHandler<DeleteStaffCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public DeleteStaffCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(DeleteStaffCommand request, CancellationToken cancellationToken)
    {
        var staff = await _context.StaffMembers
            .FirstOrDefaultAsync(s => s.Id == request.StaffId, cancellationToken);

        if (staff is null)
        {
            return Result.Failure("StaffNotFound");
        }

        _context.StaffMembers.Remove(staff);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
