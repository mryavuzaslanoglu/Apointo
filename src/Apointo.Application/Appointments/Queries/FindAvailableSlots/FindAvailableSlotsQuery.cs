using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Apointo.Application.Appointments.Dtos;
using Apointo.Application.Common.Interfaces.Persistence;
using Apointo.Application.Common.Models;
using Apointo.Domain.Staff;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Apointo.Application.Appointments.Queries.FindAvailableSlots;

public sealed record FindAvailableSlotsQuery(
    List<Guid> ServiceIds,
    Guid? PreferredStaffId,
    DateTime StartDate,
    DateTime EndDate,
    Guid BusinessId) : IRequest<Result<FindAvailableSlotsDto>>;

public sealed class FindAvailableSlotsQueryHandler : IRequestHandler<FindAvailableSlotsQuery, Result<FindAvailableSlotsDto>>
{
    private readonly IApplicationDbContext _context;

    public FindAvailableSlotsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<FindAvailableSlotsDto>> Handle(FindAvailableSlotsQuery request, CancellationToken cancellationToken)
    {
        // Validate input
        if (!request.ServiceIds.Any())
        {
            return Result<FindAvailableSlotsDto>.Failure("ServicesRequired");
        }

        if (request.StartDate >= request.EndDate)
        {
            return Result<FindAvailableSlotsDto>.Failure("InvalidDateRange");
        }

        // Get services and calculate total duration
        var services = await _context.Services
            .AsNoTracking()
            .Where(s => request.ServiceIds.Contains(s.Id) && s.BusinessId == request.BusinessId && s.IsActive)
            .ToListAsync(cancellationToken);

        if (services.Count != request.ServiceIds.Count)
        {
            return Result<FindAvailableSlotsDto>.Failure("SomeServicesNotFound");
        }

        var totalDurationInMinutes = services.Sum(s => s.DurationInMinutes + s.BufferTimeInMinutes);

        // Get staff members who can perform all selected services
        var eligibleStaffIds = await GetEligibleStaffAsync(request.ServiceIds, request.PreferredStaffId, request.BusinessId, cancellationToken);

        if (!eligibleStaffIds.Any())
        {
            return Result<FindAvailableSlotsDto>.Failure("NoEligibleStaff");
        }

        // Get staff members with their schedules
        var staffMembers = await _context.StaffMembers
            .AsNoTracking()
            .Include(s => s.Schedules)
            .Include(s => s.AvailabilityOverrides)
            .Where(s => eligibleStaffIds.Contains(s.Id) && s.IsActive)
            .ToListAsync(cancellationToken);

        // Get business operating hours
        var business = await _context.Businesses
            .AsNoTracking()
            .Include(b => b.OperatingHours)
            .FirstOrDefaultAsync(b => b.Id == request.BusinessId, cancellationToken);

        if (business is null)
        {
            return Result<FindAvailableSlotsDto>.Failure("BusinessNotFound");
        }

        // Get existing appointments for the date range
        var existingAppointments = await _context.Appointments
            .AsNoTracking()
            .Where(a => a.BusinessId == request.BusinessId &&
                       a.StartTimeUtc >= request.StartDate &&
                       a.StartTimeUtc <= request.EndDate &&
                       a.Status != Domain.Appointments.AppointmentStatus.Cancelled &&
                       a.Status != Domain.Appointments.AppointmentStatus.NoShow)
            .ToListAsync(cancellationToken);

        var availableSlots = new List<AvailableSlotDto>();

        // Generate time slots for each day
        for (var date = request.StartDate.Date; date <= request.EndDate.Date; date = date.AddDays(1))
        {
            var dayOfWeek = date.DayOfWeek;

            // Get business operating hours for this day
            var businessHours = business.OperatingHours.FirstOrDefault(h => h.DayOfWeek == dayOfWeek);
            if (businessHours?.IsClosed == true || businessHours?.OpenTime is null || businessHours?.CloseTime is null)
            {
                continue; // Business is closed on this day
            }

            var businessOpenTime = date.Add(businessHours.OpenTime.Value);
            var businessCloseTime = date.Add(businessHours.CloseTime.Value);

            foreach (var staff in staffMembers)
            {
                var staffSlots = GenerateStaffAvailableSlots(
                    staff,
                    date,
                    businessOpenTime,
                    businessCloseTime,
                    totalDurationInMinutes,
                    existingAppointments.Where(a => a.StaffId == staff.Id).ToList());

                availableSlots.AddRange(staffSlots);
            }
        }

        // Sort by start time
        availableSlots = availableSlots.OrderBy(s => s.StartTime).ToList();

        var result = new FindAvailableSlotsDto(
            request.StartDate,
            totalDurationInMinutes,
            availableSlots);

        return Result<FindAvailableSlotsDto>.Success(result);
    }

    private async Task<List<Guid>> GetEligibleStaffAsync(
        List<Guid> serviceIds,
        Guid? preferredStaffId,
        Guid businessId,
        CancellationToken cancellationToken)
    {
        if (preferredStaffId.HasValue)
        {
            // Check if preferred staff can perform all services
            var canPerformAll = await _context.StaffServices
                .Where(ss => ss.StaffId == preferredStaffId.Value && serviceIds.Contains(ss.ServiceId))
                .CountAsync(cancellationToken) == serviceIds.Count;

            if (canPerformAll)
            {
                return new List<Guid> { preferredStaffId.Value };
            }
        }

        // Get all staff who can perform all selected services
        var eligibleStaffIds = await _context.StaffServices
            .Where(ss => serviceIds.Contains(ss.ServiceId))
            .GroupBy(ss => ss.StaffId)
            .Where(g => g.Count() == serviceIds.Count)
            .Select(g => g.Key)
            .ToListAsync(cancellationToken);

        return eligibleStaffIds;
    }

    private static List<AvailableSlotDto> GenerateStaffAvailableSlots(
        Domain.Staff.Staff staff,
        DateTime date,
        DateTime businessOpenTime,
        DateTime businessCloseTime,
        int requiredDurationInMinutes,
        List<Domain.Appointments.Appointment> existingAppointments)
    {
        var availableSlots = new List<AvailableSlotDto>();
        var dayOfWeek = date.DayOfWeek;

        // Get staff schedule for this day
        var staffSchedule = staff.Schedules.FirstOrDefault(s => s.DayOfWeek == dayOfWeek);
        if (staffSchedule?.IsWorking != true || staffSchedule?.StartTime is null || staffSchedule?.EndTime is null)
        {
            return availableSlots; // Staff doesn't work on this day
        }

        var staffStartTime = date.Add(staffSchedule.StartTime.Value);
        var staffEndTime = date.Add(staffSchedule.EndTime.Value);

        // Apply business hours constraints
        var workStartTime = staffStartTime < businessOpenTime ? businessOpenTime : staffStartTime;
        var workEndTime = staffEndTime > businessCloseTime ? businessCloseTime : staffEndTime;

        // Check for availability overrides (vacations, sick days, etc.)
        var availabilityOverride = staff.AvailabilityOverrides.FirstOrDefault(o => o.Date == DateOnly.FromDateTime(date.Date));
        if (availabilityOverride?.Type == StaffAvailabilityType.Unavailable)
        {
            return availableSlots; // Staff is unavailable on this day
        }

        if (availabilityOverride?.Type == StaffAvailabilityType.AvailableOverride) // Custom availability
        {
            if (availabilityOverride.StartTime.HasValue && availabilityOverride.EndTime.HasValue)
            {
                workStartTime = date.Add(availabilityOverride.StartTime.Value);
                workEndTime = date.Add(availabilityOverride.EndTime.Value);
            }
        }

        // Generate 15-minute slots
        var slotDuration = TimeSpan.FromMinutes(15);
        var currentTime = workStartTime;

        while (currentTime.Add(TimeSpan.FromMinutes(requiredDurationInMinutes)) <= workEndTime)
        {
            var slotEndTime = currentTime.Add(TimeSpan.FromMinutes(requiredDurationInMinutes));

            // Check if this slot conflicts with existing appointments
            var hasConflict = existingAppointments.Any(a =>
                (currentTime >= a.StartTimeUtc && currentTime < a.EndTimeUtc) ||
                (slotEndTime > a.StartTimeUtc && slotEndTime <= a.EndTimeUtc) ||
                (currentTime <= a.StartTimeUtc && slotEndTime >= a.EndTimeUtc));

            if (!hasConflict)
            {
                availableSlots.Add(new AvailableSlotDto(
                    currentTime,
                    slotEndTime,
                    staff.Id,
                    staff.FullName,
                    true));
            }

            currentTime = currentTime.Add(slotDuration);
        }

        return availableSlots;
    }
}