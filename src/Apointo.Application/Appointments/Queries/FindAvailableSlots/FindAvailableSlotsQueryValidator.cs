using System;
using FluentValidation;

namespace Apointo.Application.Appointments.Queries.FindAvailableSlots;

public sealed class FindAvailableSlotsQueryValidator : AbstractValidator<FindAvailableSlotsQuery>
{
    public FindAvailableSlotsQueryValidator()
    {
        RuleFor(x => x.ServiceIds)
            .NotEmpty()
            .WithMessage("At least one service must be selected");

        RuleForEach(x => x.ServiceIds)
            .NotEmpty()
            .WithMessage("Service ID cannot be empty");

        RuleFor(x => x.StartDate)
            .GreaterThanOrEqualTo(DateTime.Today)
            .WithMessage("Start date cannot be in the past");

        RuleFor(x => x.EndDate)
            .GreaterThan(x => x.StartDate)
            .WithMessage("End date must be after start date");

        RuleFor(x => x.EndDate)
            .LessThanOrEqualTo(x => x.StartDate.AddDays(30))
            .WithMessage("Date range cannot exceed 30 days");

        RuleFor(x => x.BusinessId)
            .NotEmpty()
            .WithMessage("Business ID is required");
    }
}