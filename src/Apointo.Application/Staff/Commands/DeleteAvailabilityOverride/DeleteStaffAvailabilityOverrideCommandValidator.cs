using FluentValidation;

namespace Apointo.Application.Staff.Commands.DeleteAvailabilityOverride;

public sealed class DeleteStaffAvailabilityOverrideCommandValidator : AbstractValidator<DeleteStaffAvailabilityOverrideCommand>
{
    public DeleteStaffAvailabilityOverrideCommandValidator()
    {
        RuleFor(x => x.StaffId)
            .NotEmpty();

        RuleFor(x => x.OverrideId)
            .NotEmpty();
    }
}
