using System.Linq;
using Apointo.Domain.Identity;
using FluentValidation;

namespace Apointo.Application.Authentication.Commands.Register;

public sealed class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(8)
            .Matches("[A-Z]")
            .Matches("[a-z]")
            .Matches("\\d")
            .Matches("[!@#\\$%\\^&*(),.?\"{}|<>]");

        RuleFor(x => x.FirstName)
            .NotEmpty()
            .MaximumLength(128);

        RuleFor(x => x.LastName)
            .NotEmpty()
            .MaximumLength(128);

        RuleFor(x => x.Role)
            .NotEmpty()
            .Must(role => RoleNames.All.Contains(role))
            .WithMessage("Invalid role specified.");
    }
}