using FluentValidation;

namespace Apointo.Application.Staff.Commands.UpdateStaff;

public sealed class UpdateStaffCommandValidator : AbstractValidator<UpdateStaffCommand>
{
    public UpdateStaffCommandValidator()
    {
        RuleFor(x => x.StaffId)
            .NotEmpty();

        RuleFor(x => x.FirstName)
            .NotEmpty()
            .MaximumLength(128);

        RuleFor(x => x.LastName)
            .NotEmpty()
            .MaximumLength(128);

        RuleFor(x => x.Title)
            .MaximumLength(128);

        RuleFor(x => x.Email)
            .EmailAddress()
            .When(x => !string.IsNullOrWhiteSpace(x.Email));

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(64);
    }
}
