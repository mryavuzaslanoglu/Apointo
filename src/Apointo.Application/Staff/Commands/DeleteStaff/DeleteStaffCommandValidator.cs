using FluentValidation;

namespace Apointo.Application.Staff.Commands.DeleteStaff;

public sealed class DeleteStaffCommandValidator : AbstractValidator<DeleteStaffCommand>
{
    public DeleteStaffCommandValidator()
    {
        RuleFor(x => x.StaffId)
            .NotEmpty();
    }
}
