using System;
using FluentValidation;

namespace Apointo.Application.Appointments.Commands.CreateAppointment;

public sealed class CreateAppointmentCommandValidator : AbstractValidator<CreateAppointmentCommand>
{
    public CreateAppointmentCommandValidator()
    {
        RuleFor(x => x.BusinessId)
            .NotEmpty()
            .WithMessage("Business ID is required");

        RuleFor(x => x.CustomerId)
            .NotEmpty()
            .WithMessage("Customer ID is required");

        RuleFor(x => x.StaffId)
            .NotEmpty()
            .WithMessage("Staff ID is required");

        RuleFor(x => x.StartTimeUtc)
            .GreaterThan(DateTime.UtcNow)
            .WithMessage("Appointment start time must be in the future");

        RuleFor(x => x.ServiceIds)
            .NotEmpty()
            .WithMessage("At least one service must be selected");

        RuleForEach(x => x.ServiceIds)
            .NotEmpty()
            .WithMessage("Service ID cannot be empty");

        RuleFor(x => x.Notes)
            .MaximumLength(500)
            .WithMessage("Notes cannot exceed 500 characters");
    }
}