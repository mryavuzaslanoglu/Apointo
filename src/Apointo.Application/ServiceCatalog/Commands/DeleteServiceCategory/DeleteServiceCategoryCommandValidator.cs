using FluentValidation;

namespace Apointo.Application.ServiceCatalog.Commands.DeleteServiceCategory;

public sealed class DeleteServiceCategoryCommandValidator : AbstractValidator<DeleteServiceCategoryCommand>
{
    public DeleteServiceCategoryCommandValidator()
    {
        RuleFor(x => x.CategoryId)
            .NotEmpty();
    }
}
