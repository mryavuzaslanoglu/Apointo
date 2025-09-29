using FluentValidation;

namespace Apointo.Application.Appointments.Queries.GetCustomerAppointments;

public sealed class GetCustomerAppointmentsQueryValidator : AbstractValidator<GetCustomerAppointmentsQuery>
{
    public GetCustomerAppointmentsQueryValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty()
            .WithMessage("Customer ID is required");

        RuleFor(x => x.PageNumber)
            .GreaterThan(0)
            .WithMessage("Page number must be greater than 0");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 50)
            .WithMessage("Page size must be between 1 and 50");
    }
}