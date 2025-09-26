using FluentValidation;

namespace Apointo.Application.ServiceCatalog.Commands.UpdateService;

public sealed class UpdateServiceCommandValidator : AbstractValidator<UpdateServiceCommand>
{
    public UpdateServiceCommandValidator()
    {
        RuleFor(x => x.ServiceId)
            .NotEmpty();

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(256);

        RuleFor(x => x.Description)
            .MaximumLength(1024);

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.DurationInMinutes)
            .GreaterThan(0);

        RuleFor(x => x.BufferTimeInMinutes)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.ColorHex)
            .Matches("^#?[0-9A-Fa-f]{3,8}$")
            .When(x => !string.IsNullOrWhiteSpace(x.ColorHex));

        RuleFor(x => x.CategoryId)
            .NotEmpty();
    }
}
