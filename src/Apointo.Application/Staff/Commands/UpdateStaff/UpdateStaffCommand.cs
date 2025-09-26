using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Apointo.Application.Common.Interfaces.Persistence;
using Apointo.Application.Common.Models;
using Apointo.Application.Staff.Dtos;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Apointo.Application.Staff.Commands.UpdateStaff;

public sealed record UpdateStaffCommand(
    Guid StaffId,
    string FirstName,
    string LastName,
    string? Title,
    string? Email,
    string? PhoneNumber,
    bool IsActive,
    Guid? UserId,
    DateTime? HiredAtUtc,
    DateTime? TerminatedAtUtc) : IRequest<Result<StaffDetailDto>>;

public sealed class UpdateStaffCommandHandler : IRequestHandler<UpdateStaffCommand, Result<StaffDetailDto>>
{
    private readonly IApplicationDbContext _context;

    public UpdateStaffCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<StaffDetailDto>> Handle(UpdateStaffCommand request, CancellationToken cancellationToken)
    {
        var staff = await _context.StaffMembers
            .Include(s => s.Schedules)
            .Include(s => s.AvailabilityOverrides)
            .FirstOrDefaultAsync(s => s.Id == request.StaffId, cancellationToken);

        if (staff is null)
        {
            return Result<StaffDetailDto>.Failure("StaffNotFound");
        }

        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            var emailExists = await _context.StaffMembers
                .AnyAsync(s => s.BusinessId == staff.BusinessId && s.Email == request.Email && s.Id != request.StaffId, cancellationToken);

            if (emailExists)
            {
                return Result<StaffDetailDto>.Failure("StaffEmailAlreadyExists");
            }
        }

        staff.Update(
            request.FirstName,
            request.LastName,
            request.Email,
            request.PhoneNumber,
            request.Title,
            request.IsActive,
            request.UserId,
            request.HiredAtUtc,
            request.TerminatedAtUtc);

        await _context.SaveChangesAsync(cancellationToken);

        var detail = new StaffDetailDto(
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
            staff.Schedules
                .OrderBy(s => s.DayOfWeek)
                .Select(s => new StaffScheduleDto(
                    s.DayOfWeek,
                    s.IsWorking,
                    s.StartTime?.ToString(@"hh\:mm"),
                    s.EndTime?.ToString(@"hh\:mm")))
                .ToList(),
            staff.AvailabilityOverrides
                .OrderByDescending(o => o.Date)
                .Select(o => new StaffAvailabilityOverrideDto(
                    o.Id.ToString(),
                    o.Date,
                    o.Type,
                    o.StartTime?.ToString(@"hh\:mm"),
                    o.EndTime?.ToString(@"hh\:mm"),
                    o.Reason))
                .ToList());

        return Result<StaffDetailDto>.Success(detail);
    }
}
