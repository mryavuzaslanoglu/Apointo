using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Apointo.Application.Common.Interfaces.Persistence;
using Apointo.Application.Common.Models;
using Apointo.Application.Staff.Dtos;
using Apointo.Domain.Staff;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Apointo.Application.Staff.Commands.CreateAvailabilityOverride;

public sealed record CreateStaffAvailabilityOverrideCommand(
    Guid StaffId,
    DateOnly Date,
    StaffAvailabilityType Type,
    string? StartTime,
    string? EndTime,
    string? Reason) : IRequest<Result<StaffAvailabilityOverrideDto>>;

public sealed class CreateStaffAvailabilityOverrideCommandHandler : IRequestHandler<CreateStaffAvailabilityOverrideCommand, Result<StaffAvailabilityOverrideDto>>
{
    private static readonly string[] TimeFormats = { @"hh\:mm", @"h\:mm" };
    private readonly IApplicationDbContext _context;

    public CreateStaffAvailabilityOverrideCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<StaffAvailabilityOverrideDto>> Handle(CreateStaffAvailabilityOverrideCommand request, CancellationToken cancellationToken)
    {
        var staff = await _context.StaffMembers
            .Include(s => s.AvailabilityOverrides)
            .FirstOrDefaultAsync(s => s.Id == request.StaffId, cancellationToken);

        if (staff is null)
        {
            return Result<StaffAvailabilityOverrideDto>.Failure("StaffNotFound");
        }

        if (staff.AvailabilityOverrides.Any(o => o.Date == request.Date && o.Type == request.Type))
        {
            return Result<StaffAvailabilityOverrideDto>.Failure("AvailabilityOverrideExists");
        }

        TimeSpan? startTime = null;
        TimeSpan? endTime = null;

        if (request.Type == StaffAvailabilityType.AvailableOverride)
        {
            startTime = ParseTime(request.StartTime, "Start time is required for availability override.");
            endTime = ParseTime(request.EndTime, "End time is required for availability override.");

            if (endTime <= startTime)
            {
                return Result<StaffAvailabilityOverrideDto>.Failure("AvailabilityEndBeforeStart");
            }
        }

        if (request.Type == StaffAvailabilityType.Unavailable && !string.IsNullOrWhiteSpace(request.StartTime))
        {
            startTime = ParseTime(request.StartTime, "Invalid start time format.");
        }

        if (request.Type == StaffAvailabilityType.Unavailable && !string.IsNullOrWhiteSpace(request.EndTime))
        {
            endTime = ParseTime(request.EndTime, "Invalid end time format.");
        }

        var overrideEntity = StaffAvailabilityOverride.Create(
            request.StaffId,
            request.Date,
            request.Type,
            startTime,
            endTime,
            request.Reason);

        await _context.StaffAvailabilityOverrides.AddAsync(overrideEntity, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        var dto = new StaffAvailabilityOverrideDto(
            overrideEntity.Id.ToString(),
            overrideEntity.Date,
            overrideEntity.Type,
            overrideEntity.StartTime?.ToString(@"hh\:mm"),
            overrideEntity.EndTime?.ToString(@"hh\:mm"),
            overrideEntity.Reason);

        return Result<StaffAvailabilityOverrideDto>.Success(dto);
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
