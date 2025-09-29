using System;
using System.Threading;
using System.Threading.Tasks;
using Apointo.Application.Common.Interfaces.Persistence;
using Apointo.Application.Common.Models;
using Apointo.Domain.Appointments;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Apointo.Application.Appointments.Commands.CancelAppointment;

public sealed record CancelAppointmentCommand(
    Guid AppointmentId,
    Guid UserId,
    string? CancellationReason) : IRequest<Result>;

public sealed class CancelAppointmentCommandHandler : IRequestHandler<CancelAppointmentCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public CancelAppointmentCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(CancelAppointmentCommand request, CancellationToken cancellationToken)
    {
        var appointment = await _context.Appointments
            .FirstOrDefaultAsync(a => a.Id == request.AppointmentId, cancellationToken);

        if (appointment is null)
        {
            return Result.Failure("AppointmentNotFound");
        }

        // Check if user has permission to cancel this appointment
        // Customer can cancel their own appointments
        if (appointment.CustomerId != request.UserId)
        {
            return Result.Failure("UnauthorizedAccess");
        }

        // Check if appointment can be cancelled
        if (appointment.Status == AppointmentStatus.Completed)
        {
            return Result.Failure("CannotCancelCompletedAppointment");
        }

        if (appointment.Status == AppointmentStatus.Cancelled)
        {
            return Result.Failure("AppointmentAlreadyCancelled");
        }

        // Check cancellation policy (e.g., minimum 24 hours notice)
        var hoursUntilAppointment = (appointment.StartTimeUtc - DateTime.UtcNow).TotalHours;
        if (hoursUntilAppointment < 24)
        {
            return Result.Failure("CancellationTooLate");
        }

        try
        {
            appointment.Cancel(request.CancellationReason, request.UserId);
            await _context.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(ex.Message);
        }
    }
}