using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Apointo.Application.Appointments.Dtos;
using Apointo.Application.Common.Interfaces.Persistence;
using Apointo.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Apointo.Application.Appointments.Queries.GetCalendarAppointments;

public sealed record GetCalendarAppointmentsQuery(
    Guid BusinessId,
    DateTime StartDate,
    DateTime EndDate,
    List<Guid>? StaffIds = null) : IRequest<Result<CalendarViewDto>>;

public sealed class GetCalendarAppointmentsQueryHandler : IRequestHandler<GetCalendarAppointmentsQuery, Result<CalendarViewDto>>
{
    private readonly IApplicationDbContext _context;

    public GetCalendarAppointmentsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<CalendarViewDto>> Handle(GetCalendarAppointmentsQuery request, CancellationToken cancellationToken)
    {
        // Validate date range
        if (request.StartDate >= request.EndDate)
        {
            return Result<CalendarViewDto>.Failure("InvalidDateRange");
        }

        if ((request.EndDate - request.StartDate).TotalDays > 90)
        {
            return Result<CalendarViewDto>.Failure("DateRangeTooLarge");
        }

        // Build appointments query
        var appointmentsQuery = _context.Appointments
            .AsNoTracking()
            .Include(a => a.AppointmentServices)
                .ThenInclude(aps => aps.Service)
            .Where(a => a.BusinessId == request.BusinessId &&
                       a.StartTimeUtc >= request.StartDate &&
                       a.StartTimeUtc <= request.EndDate);

        // Filter by staff if specified
        if (request.StaffIds?.Any() == true)
        {
            appointmentsQuery = appointmentsQuery.Where(a => request.StaffIds.Contains(a.StaffId));
        }

        var appointments = await appointmentsQuery
            .OrderBy(a => a.StartTimeUtc)
            .ToListAsync(cancellationToken);

        // Get staff and customer information
        var staffIds = appointments.Select(a => a.StaffId).Distinct().ToList();
        var customerIds = appointments.Select(a => a.CustomerId).Distinct().ToList();

        var staffMembers = await _context.StaffMembers
            .AsNoTracking()
            .Where(s => staffIds.Contains(s.Id))
            .ToDictionaryAsync(s => s.Id, s => new { s.FullName, s.Title }, cancellationToken);

        // For customers, we would need a Users table or similar
        // For now, using customer ID as name placeholder
        var customers = customerIds.ToDictionary(id => id, id => $"Customer {id.ToString()[..8]}");

        // Get all staff for the business (for calendar view)
        var allStaff = await _context.StaffMembers
            .AsNoTracking()
            .Where(s => s.BusinessId == request.BusinessId && s.IsActive)
            .Select(s => new StaffCalendarInfoDto(
                s.Id.ToString(),
                s.FullName,
                null)) // Color can be assigned dynamically
            .ToListAsync(cancellationToken);

        // Map appointments to calendar DTOs
        var calendarAppointments = appointments.Select(appointment =>
        {
            var serviceNames = appointment.AppointmentServices
                .Select(aps => aps.Service?.Name ?? "Unknown Service")
                .ToList();

            var staffInfo = staffMembers.GetValueOrDefault(appointment.StaffId);
            var customerName = customers.GetValueOrDefault(appointment.CustomerId, "Unknown Customer");

            // Create title from customer name and services
            var title = $"{customerName} - {string.Join(", ", serviceNames)}";

            return new CalendarAppointmentDto(
                appointment.Id.ToString(),
                title,
                appointment.StartTimeUtc,
                appointment.EndTimeUtc,
                appointment.StaffId.ToString(),
                staffInfo?.FullName ?? "Unknown Staff",
                appointment.CustomerId.ToString(),
                customerName,
                appointment.Status.ToString(),
                appointment.TotalPrice,
                appointment.Notes,
                serviceNames,
                GetStatusColor(appointment.Status.ToString()));
        }).ToList();

        var result = new CalendarViewDto(
            request.StartDate,
            request.EndDate,
            calendarAppointments,
            allStaff);

        return Result<CalendarViewDto>.Success(result);
    }

    private static string GetStatusColor(string status)
    {
        return status switch
        {
            "Scheduled" => "#007bff",    // Blue
            "Confirmed" => "#28a745",    // Green
            "InProgress" => "#ffc107",   // Yellow
            "Completed" => "#6c757d",    // Gray
            "Cancelled" => "#dc3545",    // Red
            "NoShow" => "#fd7e14",       // Orange
            "Rescheduled" => "#6f42c1",  // Purple
            _ => "#007bff"
        };
    }
}