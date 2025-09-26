using Apointo.Domain.Staff;
using FluentValidation;

namespace Apointo.Application.Staff.Commands.CreateAvailabilityOverride;

public sealed class CreateStaffAvailabilityOverrideCommandValidator : AbstractValidator<CreateStaffAvailabilityOverrideCommand>
{
    public CreateStaffAvailabilityOverrideCommandValidator()
    {
        RuleFor(x => x.StaffId)
            .NotEmpty();

        RuleFor(x => x.Date)
            .NotEmpty();

        RuleFor(x => x.Type)
            .IsInEnum();

        RuleFor(x => x.StartTime)
            .NotEmpty()
            .Matches("^\\d{1,2}:[0-5]\\d$")
            .When(x => x.Type == StaffAvailabilityType.AvailableOverride);

        RuleFor(x => x.EndTime)
            .NotEmpty()
            .Matches("^\\d{1,2}:[0-5]\\d$")
            .When(x => x.Type == StaffAvailabilityType.AvailableOverride);

        RuleFor(x => x.Reason)
            .MaximumLength(256);
    }
}
