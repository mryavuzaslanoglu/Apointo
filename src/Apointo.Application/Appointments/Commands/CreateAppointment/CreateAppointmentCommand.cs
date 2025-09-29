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

namespace Apointo.Application.Appointments.Commands.CreateAppointment;

public sealed record CreateAppointmentCommand(
    Guid BusinessId,
    Guid CustomerId,
    Guid StaffId,
    DateTime StartTimeUtc,
    List<Guid> ServiceIds,
    string? Notes) : IRequest<Result<AppointmentDto>>;

public sealed class CreateAppointmentCommandHandler : IRequestHandler<CreateAppointmentCommand, Result<AppointmentDto>>
{
    private readonly IApplicationDbContext _context;

    public CreateAppointmentCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<AppointmentDto>> Handle(CreateAppointmentCommand request, CancellationToken cancellationToken)
    {
        // Validate services exist and are active
        var services = await _context.Services
            .AsNoTracking()
            .Where(s => request.ServiceIds.Contains(s.Id) &&
                       s.BusinessId == request.BusinessId &&
                       s.IsActive)
            .ToListAsync(cancellationToken);

        if (services.Count != request.ServiceIds.Count)
        {
            return Result<AppointmentDto>.Failure("SomeServicesNotFound");
        }

        // Validate staff exists and is active
        var staff = await _context.StaffMembers
            .AsNoTracking()
            .Include(s => s.StaffServices)
            .FirstOrDefaultAsync(s => s.Id == request.StaffId &&
                                    s.BusinessId == request.BusinessId &&
                                    s.IsActive, cancellationToken);

        if (staff is null)
        {
            return Result<AppointmentDto>.Failure("StaffNotFound");
        }

        // Validate staff can perform all selected services
        var staffServiceIds = staff.StaffServices.Select(ss => ss.ServiceId).ToList();
        if (!request.ServiceIds.All(serviceId => staffServiceIds.Contains(serviceId)))
        {
            return Result<AppointmentDto>.Failure("StaffCannotPerformAllServices");
        }

        // Calculate total duration and price
        var totalDurationInMinutes = services.Sum(s => s.DurationInMinutes + s.BufferTimeInMinutes);
        var totalPrice = services.Sum(s => s.Price);
        var endTimeUtc = request.StartTimeUtc.AddMinutes(totalDurationInMinutes);

        // Check for time slot availability
        var hasConflict = await _context.Appointments
            .Where(a => a.StaffId == request.StaffId &&
                       a.Status != AppointmentStatus.Cancelled &&
                       a.Status != AppointmentStatus.NoShow &&
                       ((request.StartTimeUtc >= a.StartTimeUtc && request.StartTimeUtc < a.EndTimeUtc) ||
                        (endTimeUtc > a.StartTimeUtc && endTimeUtc <= a.EndTimeUtc) ||
                        (request.StartTimeUtc <= a.StartTimeUtc && endTimeUtc >= a.EndTimeUtc)))
            .AnyAsync(cancellationToken);

        if (hasConflict)
        {
            return Result<AppointmentDto>.Failure("TimeSlotNotAvailable");
        }

        // Create appointment
        var appointment = Appointment.Create(
            request.BusinessId,
            request.CustomerId,
            request.StaffId,
            request.StartTimeUtc,
            endTimeUtc,
            totalPrice,
            request.Notes);

        // Add services to appointment
        foreach (var service in services)
        {
            appointment.AddService(service.Id, service.Price, service.DurationInMinutes + service.BufferTimeInMinutes);
        }

        _context.Appointments.Add(appointment);
        await _context.SaveChangesAsync(cancellationToken);

        // Create response DTO
        var appointmentServices = services.Select(s => new AppointmentServiceDto(
            s.Id.ToString(),
            s.Name,
            s.Price,
            s.DurationInMinutes)).ToList();

        var result = new AppointmentDto(
            appointment.Id.ToString(),
            appointment.CustomerId.ToString(),
            appointment.StaffId.ToString(),
            staff.FullName,
            appointment.StartTimeUtc,
            appointment.EndTimeUtc,
            appointment.TotalPrice,
            appointment.Status.ToString(),
            appointment.Notes,
            appointmentServices);

        return Result<AppointmentDto>.Success(result);
    }
}