using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Apointo.Application.Common.Interfaces.Persistence;
using Apointo.Application.Common.Models;
using Apointo.Application.Staff.Dtos;
using StaffScheduleEntity = Apointo.Domain.Staff.StaffSchedule;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Apointo.Application.Staff.Commands.UpdateStaffSchedule;

public sealed record UpdateStaffScheduleCommand(
    Guid StaffId,
    IReadOnlyCollection<StaffScheduleInput> Schedules) : IRequest<Result<IReadOnlyCollection<StaffScheduleDto>>>;

public sealed record StaffScheduleInput(
    DayOfWeek DayOfWeek,
    bool IsWorking,
    string? StartTime,
    string? EndTime);

public sealed class UpdateStaffScheduleCommandHandler : IRequestHandler<UpdateStaffScheduleCommand, Result<IReadOnlyCollection<StaffScheduleDto>>>
{
    private static readonly string[] TimeFormats = { @"hh\:mm", @"h\:mm" };
    private readonly IApplicationDbContext _context;

    public UpdateStaffScheduleCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<IReadOnlyCollection<StaffScheduleDto>>> Handle(UpdateStaffScheduleCommand request, CancellationToken cancellationToken)
    {
        var staff = await _context.StaffMembers
            .Include(s => s.Schedules)
            .FirstOrDefaultAsync(s => s.Id == request.StaffId, cancellationToken);

        if (staff is null)
        {
            return Result<IReadOnlyCollection<StaffScheduleDto>>.Failure("StaffNotFound");
        }

        var schedules = request.Schedules
            .OrderBy(s => s.DayOfWeek)
            .Select(input =>
            {
                if (input.IsWorking)
                {
                    var start = ParseTime(input.StartTime, "Start time is required when staff is working.");
                    var end = ParseTime(input.EndTime, "End time is required when staff is working.");

                    if (end <= start)
                    {
                        throw new InvalidOperationException("End time must be later than start time.");
                    }

                    return StaffScheduleEntity.Create(staff.Id, input.DayOfWeek, true, start, end);
                }

                return StaffScheduleEntity.Create(staff.Id, input.DayOfWeek, false, null, null);
            })
            .ToList();

        staff.SetSchedules(schedules);
        await _context.SaveChangesAsync(cancellationToken);

        var result = staff.Schedules
            .OrderBy(s => s.DayOfWeek)
            .Select(s => new StaffScheduleDto(
                s.DayOfWeek,
                s.IsWorking,
                s.StartTime?.ToString(@"hh\:mm"),
                s.EndTime?.ToString(@"hh\:mm")))
            .ToList();

        return Result<IReadOnlyCollection<StaffScheduleDto>>.Success(result);
    }

    private static TimeSpan ParseTime(string? value, string errorMessage)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException(errorMessage);
        }

        if (TimeSpan.TryParseExact(value, TimeFormats, CultureInfo.InvariantCulture, out var time))
        {
            return time;
        }

        throw new InvalidOperationException($"Invalid time format: {value}");
    }
}
