using System.Collections.Generic;
using System.Linq;
using FluentValidation;

namespace Apointo.Application.Staff.Commands.UpdateStaffSchedule;

public sealed class UpdateStaffScheduleCommandValidator : AbstractValidator<UpdateStaffScheduleCommand>
{
    public UpdateStaffScheduleCommandValidator()
    {
        RuleFor(x => x.StaffId)
            .NotEmpty();

        RuleFor(x => x.Schedules)
            .NotNull()
            .Must(HaveUniqueDays)
            .WithMessage("Each day of week must appear only once.");

        RuleForEach(x => x.Schedules)
            .SetValidator(new StaffScheduleInputValidator());
    }

    private static bool HaveUniqueDays(IReadOnlyCollection<StaffScheduleInput>? schedules)
    {
        if (schedules is null)
        {
            return false;
        }

        return schedules
            .GroupBy(s => s.DayOfWeek)
            .All(g => g.Count() == 1);
    }

    private sealed class StaffScheduleInputValidator : AbstractValidator<StaffScheduleInput>
    {
        public StaffScheduleInputValidator()
        {
            RuleFor(x => x.DayOfWeek)
                .IsInEnum();

            RuleFor(x => x.StartTime)
                .NotEmpty()
                .Matches("^\\d{1,2}:[0-5]\\d$")
                .When(x => x.IsWorking);

            RuleFor(x => x.EndTime)
                .NotEmpty()
                .Matches("^\\d{1,2}:[0-5]\\d$")
                .When(x => x.IsWorking);
        }
    }
}
