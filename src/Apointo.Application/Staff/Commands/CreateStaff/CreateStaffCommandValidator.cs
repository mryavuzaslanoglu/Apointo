using FluentValidation;

namespace Apointo.Application.Staff.Commands.CreateStaff;

public sealed class CreateStaffCommandValidator : AbstractValidator<CreateStaffCommand>
{
    public CreateStaffCommandValidator()
    {
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
