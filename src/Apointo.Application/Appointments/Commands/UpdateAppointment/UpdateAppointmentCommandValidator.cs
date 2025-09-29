using System;
using FluentValidation;

namespace Apointo.Application.Appointments.Commands.UpdateAppointment;

public sealed class UpdateAppointmentCommandValidator : AbstractValidator<UpdateAppointmentCommand>
{
    public UpdateAppointmentCommandValidator()
    {
        RuleFor(x => x.AppointmentId)
            .NotEmpty()
            .WithMessage("Appointment ID is required");

        RuleFor(x => x.NewStartTimeUtc)
            .GreaterThan(DateTime.UtcNow)
            .WithMessage("New start time must be in the future")
            .When(x => x.NewStartTimeUtc.HasValue);

        RuleFor(x => x.NewEndTimeUtc)
            .GreaterThan(x => x.NewStartTimeUtc)
            .WithMessage("End time must be after start time")
            .When(x => x.NewStartTimeUtc.HasValue && x.NewEndTimeUtc.HasValue);

        RuleFor(x => x.Notes)
            .MaximumLength(500)
            .WithMessage("Notes cannot exceed 500 characters")
            .When(x => x.Notes is not null);

        RuleFor(x => x.NewStaffId)
            .NotEmpty()
            .WithMessage("Staff ID cannot be empty")
            .When(x => x.NewStaffId.HasValue);
    }
}