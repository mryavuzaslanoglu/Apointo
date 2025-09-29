using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Apointo.Application.Appointments.Dtos;
using Apointo.Application.Common.Interfaces.Persistence;
using Apointo.Application.Common.Models;
using Apointo.Domain.Appointments;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Apointo.Application.Appointments.Queries.GetCustomerAppointments;

public sealed record GetCustomerAppointmentsQuery(
    Guid CustomerId,
    bool IncludePast = false,
    int PageNumber = 1,
    int PageSize = 10) : IRequest<Result<List<AppointmentDto>>>;

public sealed class GetCustomerAppointmentsQueryHandler : IRequestHandler<GetCustomerAppointmentsQuery, Result<List<AppointmentDto>>>
{
    private readonly IApplicationDbContext _context;

    public GetCustomerAppointmentsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<AppointmentDto>>> Handle(GetCustomerAppointmentsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Appointments
            .AsNoTracking()
            .Include(a => a.AppointmentServices)
                .ThenInclude(ass => ass.Service)
            .Where(a => a.CustomerId == request.CustomerId);

        // Filter by date if needed
        if (!request.IncludePast)
        {
            query = query.Where(a => a.StartTimeUtc >= DateTime.UtcNow);
        }

        // Apply pagination
        var appointments = await query
            .OrderBy(a => a.StartTimeUtc)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        // Get staff information
        var staffIds = appointments.Select(a => a.StaffId).Distinct().ToList();
        var staffMembers = await _context.StaffMembers
            .AsNoTracking()
            .Where(s => staffIds.Contains(s.Id))
            .ToDictionaryAsync(s => s.Id, s => s.FullName, cancellationToken);

        // Map to DTOs
        var appointmentDtos = appointments.Select(appointment =>
        {
            var appointmentServices = appointment.AppointmentServices.Select(aps =>
                new AppointmentServiceDto(
                    aps.ServiceId.ToString(),
                    aps.Service?.Name ?? "Unknown Service",
                    aps.Price,
                    aps.DurationInMinutes)).ToList();

            return new AppointmentDto(
                appointment.Id.ToString(),
                appointment.CustomerId.ToString(),
                appointment.StaffId.ToString(),
                staffMembers.GetValueOrDefault(appointment.StaffId, "Unknown Staff"),
                appointment.StartTimeUtc,
                appointment.EndTimeUtc,
                appointment.TotalPrice,
                appointment.Status.ToString(),
                appointment.Notes,
                appointmentServices);
        }).ToList();

        return Result<List<AppointmentDto>>.Success(appointmentDtos);
    }
}