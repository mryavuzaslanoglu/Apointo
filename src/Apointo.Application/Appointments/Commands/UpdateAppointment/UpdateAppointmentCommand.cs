using System;
using System.Threading;
using System.Threading.Tasks;
using Apointo.Application.Common.Interfaces.Persistence;
using Apointo.Application.Common.Models;
using Apointo.Domain.Appointments;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Apointo.Application.Appointments.Commands.UpdateAppointment;

public sealed record UpdateAppointmentCommand(
    Guid AppointmentId,
    DateTime? NewStartTimeUtc = null,
    DateTime? NewEndTimeUtc = null,
    Guid? NewStaffId = null,
    string? Notes = null,
    AppointmentStatus? Status = null) : IRequest<Result>;

public sealed class UpdateAppointmentCommandHandler : IRequestHandler<UpdateAppointmentCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public UpdateAppointmentCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(UpdateAppointmentCommand request, CancellationToken cancellationToken)
    {
        var appointment = await _context.Appointments
            .FirstOrDefaultAsync(a => a.Id == request.AppointmentId, cancellationToken);

        if (appointment is null)
        {
            return Result.Failure("AppointmentNotFound");
        }

        // Check if appointment can be updated
        if (appointment.Status == AppointmentStatus.Completed)
        {
            return Result.Failure("CannotUpdateCompletedAppointment");
        }

        if (appointment.Status == AppointmentStatus.Cancelled)
        {
            return Result.Failure("CannotUpdateCancelledAppointment");
        }

        try
        {
            // Update schedule if provided
            if (request.NewStartTimeUtc.HasValue && request.NewEndTimeUtc.HasValue)
            {
                // Check for conflicts with other appointments
                if (request.NewStaffId.HasValue && request.NewStaffId != appointment.StaffId)
                {
                    // Changing staff - check new staff availability
                    var hasConflict = await _context.Appointments
                        .Where(a => a.Id != request.AppointmentId &&
                                   a.StaffId == request.NewStaffId &&
                                   a.Status != AppointmentStatus.Cancelled &&
                                   a.Status != AppointmentStatus.NoShow &&
                                   ((request.NewStartTimeUtc >= a.StartTimeUtc && request.NewStartTimeUtc < a.EndTimeUtc) ||
                                    (request.NewEndTimeUtc > a.StartTimeUtc && request.NewEndTimeUtc <= a.EndTimeUtc) ||
                                    (request.NewStartTimeUtc <= a.StartTimeUtc && request.NewEndTimeUtc >= a.EndTimeUtc)))
                        .AnyAsync(cancellationToken);

                    if (hasConflict)
                    {
                        return Result.Failure("NewStaffNotAvailableAtRequestedTime");
                    }

                    // Update staff ID - note: this might need additional validation
                    // to ensure new staff can perform the services
                }
                else
                {
                    // Same staff, different time - check availability
                    var hasConflict = await _context.Appointments
                        .Where(a => a.Id != request.AppointmentId &&
                                   a.StaffId == appointment.StaffId &&
                                   a.Status != AppointmentStatus.Cancelled &&
                                   a.Status != AppointmentStatus.NoShow &&
                                   ((request.NewStartTimeUtc >= a.StartTimeUtc && request.NewStartTimeUtc < a.EndTimeUtc) ||
                                    (request.NewEndTimeUtc > a.StartTimeUtc && request.NewEndTimeUtc <= a.EndTimeUtc) ||
                                    (request.NewStartTimeUtc <= a.StartTimeUtc && request.NewEndTimeUtc >= a.EndTimeUtc)))
                        .AnyAsync(cancellationToken);

                    if (hasConflict)
                    {
                        return Result.Failure("TimeSlotNotAvailable");
                    }
                }

                appointment.UpdateSchedule(request.NewStartTimeUtc.Value, request.NewEndTimeUtc.Value);
            }

            // Update notes
            if (request.Notes is not null)
            {
                appointment.UpdateNotes(request.Notes);
            }

            // Update status
            if (request.Status.HasValue && request.Status != appointment.Status)
            {
                switch (request.Status.Value)
                {
                    case AppointmentStatus.Confirmed:
                        appointment.Confirm();
                        break;
                    case AppointmentStatus.InProgress:
                        appointment.StartService();
                        break;
                    case AppointmentStatus.Completed:
                        appointment.Complete();
                        break;
                    case AppointmentStatus.NoShow:
                        appointment.MarkAsNoShow();
                        break;
                    // Other status changes handled by specific commands
                }
            }

            await _context.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(ex.Message);
        }
    }
}