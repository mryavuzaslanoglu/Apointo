using FluentValidation;

namespace Apointo.Application.Appointments.Commands.CancelAppointment;

public sealed class CancelAppointmentCommandValidator : AbstractValidator<CancelAppointmentCommand>
{
    public CancelAppointmentCommandValidator()
    {
        RuleFor(x => x.AppointmentId)
            .NotEmpty()
            .WithMessage("Appointment ID is required");

        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID is required");

        RuleFor(x => x.CancellationReason)
            .MaximumLength(500)
            .WithMessage("Cancellation reason cannot exceed 500 characters");
    }
}