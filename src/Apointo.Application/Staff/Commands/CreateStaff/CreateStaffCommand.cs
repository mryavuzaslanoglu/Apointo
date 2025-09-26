using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Apointo.Application.Common.Interfaces.Persistence;
using Apointo.Application.Common.Models;
using Apointo.Application.Staff.Dtos;
using StaffEntity = Apointo.Domain.Staff.Staff;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Apointo.Application.Staff.Commands.CreateStaff;

public sealed record CreateStaffCommand(
    string FirstName,
    string LastName,
    string? Title,
    string? Email,
    string? PhoneNumber,
    bool IsActive,
    Guid? UserId,
    DateTime? HiredAtUtc) : IRequest<Result<StaffDetailDto>>;

public sealed class CreateStaffCommandHandler : IRequestHandler<CreateStaffCommand, Result<StaffDetailDto>>
{
    private readonly IApplicationDbContext _context;

    public CreateStaffCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<StaffDetailDto>> Handle(CreateStaffCommand request, CancellationToken cancellationToken)
    {
        var businessId = await _context.Businesses
            .AsNoTracking()
            .Select(b => b.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (businessId == Guid.Empty)
        {
            return Result<StaffDetailDto>.Failure("BusinessNotFound");
        }

        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            var emailExists = await _context.StaffMembers
                .AnyAsync(s => s.BusinessId == businessId && s.Email == request.Email, cancellationToken);

            if (emailExists)
            {
                return Result<StaffDetailDto>.Failure("StaffEmailAlreadyExists");
            }
        }

        var staff = StaffEntity.Create(
            businessId,
            request.FirstName,
            request.LastName,
            request.Email,
            request.PhoneNumber,
            request.Title,
            request.UserId,
            request.HiredAtUtc);

        if (!request.IsActive)
        {
            staff.Update(
                request.FirstName,
                request.LastName,
                request.Email,
                request.PhoneNumber,
                request.Title,
                false,
                request.UserId,
                request.HiredAtUtc,
                null);
        }

        await _context.StaffMembers.AddAsync(staff, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        var created = new StaffDetailDto(
            staff.Id.ToString(),
            staff.FirstName,
            staff.LastName,
            staff.FullName,
            staff.Title,
            staff.Email,
            staff.PhoneNumber,
            staff.IsActive,
            staff.UserId?.ToString(),
            staff.HiredAtUtc,
            staff.TerminatedAtUtc,
            Array.Empty<StaffScheduleDto>(),
            Array.Empty<StaffAvailabilityOverrideDto>());

        return Result<StaffDetailDto>.Success(created);
    }
}
