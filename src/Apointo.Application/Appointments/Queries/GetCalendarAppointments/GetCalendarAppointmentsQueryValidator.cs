using System;
using FluentValidation;

namespace Apointo.Application.Appointments.Queries.GetCalendarAppointments;

public sealed class GetCalendarAppointmentsQueryValidator : AbstractValidator<GetCalendarAppointmentsQuery>
{
    public GetCalendarAppointmentsQueryValidator()
    {
        RuleFor(x => x.BusinessId)
            .NotEmpty()
            .WithMessage("Business ID is required");

        RuleFor(x => x.StartDate)
            .LessThan(x => x.EndDate)
            .WithMessage("Start date must be before end date");

        RuleFor(x => x.EndDate)
            .GreaterThan(x => x.StartDate)
            .WithMessage("End date must be after start date");

        RuleFor(x => x)
            .Must(x => (x.EndDate - x.StartDate).TotalDays <= 90)
            .WithMessage("Date range cannot exceed 90 days");

        RuleForEach(x => x.StaffIds)
            .NotEmpty()
            .WithMessage("Staff ID cannot be empty")
            .When(x => x.StaffIds != null);
    }
}