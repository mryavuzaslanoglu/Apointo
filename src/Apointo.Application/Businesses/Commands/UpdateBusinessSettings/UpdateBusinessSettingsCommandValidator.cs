using System;
using System.Collections.Generic;
using System.Linq;
using Apointo.Application.Businesses.Dtos;
using FluentValidation;

namespace Apointo.Application.Businesses.Commands.UpdateBusinessSettings;

public sealed class UpdateBusinessSettingsCommandValidator : AbstractValidator<UpdateBusinessSettingsCommand>
{
    public UpdateBusinessSettingsCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(256);

        RuleFor(x => x.Description)
            .MaximumLength(1024)
            .When(x => !string.IsNullOrWhiteSpace(x.Description));

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(64);

        RuleFor(x => x.Email)
            .EmailAddress()
            .When(x => !string.IsNullOrWhiteSpace(x.Email));

        RuleFor(x => x.WebsiteUrl)
            .MaximumLength(512);

        RuleFor(x => x.Address)
            .SetValidator(new AddressValidator()!)
            .When(x => x.Address is not null);

        RuleFor(x => x.OperatingHours)
            .NotNull()
            .Must(HaveUniqueDays)
            .WithMessage("Operating hours must contain unique days of week.");

        RuleForEach(x => x.OperatingHours)
            .SetValidator(new OperatingHourValidator());
    }

    private static bool HaveUniqueDays(IReadOnlyCollection<BusinessOperatingHourInput>? operatingHours)
    {
        if (operatingHours is null)
        {
            return false;
        }

        return operatingHours
            .GroupBy(x => x.DayOfWeek)
            .All(g => g.Count() == 1);
    }

    private sealed class AddressValidator : AbstractValidator<BusinessAddressDto>
    {
        public AddressValidator()
        {
            RuleFor(x => x.Line1)
                .MaximumLength(256);

            RuleFor(x => x.Line2)
                .MaximumLength(256);

            RuleFor(x => x.City)
                .MaximumLength(128);

            RuleFor(x => x.State)
                .MaximumLength(128);

            RuleFor(x => x.PostalCode)
                .MaximumLength(32);

            RuleFor(x => x.Country)
                .MaximumLength(128);
        }
    }

    private sealed class OperatingHourValidator : AbstractValidator<BusinessOperatingHourInput>
    {
        public OperatingHourValidator()
        {
            RuleFor(x => x.DayOfWeek)
                .IsInEnum();

            RuleFor(x => x)
                .Must(x => !x.IsClosed || (string.IsNullOrWhiteSpace(x.OpenTime) && string.IsNullOrWhiteSpace(x.CloseTime)))
                .WithMessage("Closed days should not provide time ranges.");

            RuleFor(x => x.OpenTime)
                .NotEmpty()
                .Matches("^\\d{1,2}:[0-5]\\d$")
                .When(x => !x.IsClosed)
                .WithMessage("Open time must be in HH:mm format.");

            RuleFor(x => x.CloseTime)
                .NotEmpty()
                .Matches("^\\d{1,2}:[0-5]\\d$")
                .When(x => !x.IsClosed)
                .WithMessage("Close time must be in HH:mm format.");

            RuleFor(x => x)
                .Must(x => x.IsClosed || CompareTimes(x.OpenTime, x.CloseTime))
                .WithMessage("Close time must be later than open time.")
                .When(x => !x.IsClosed);
        }

        private static bool CompareTimes(string? open, string? close)
        {
            if (string.IsNullOrWhiteSpace(open) || string.IsNullOrWhiteSpace(close))
            {
                return false;
            }

            if (!TimeSpan.TryParse(open, out var openTime))
            {
                return false;
            }

            if (!TimeSpan.TryParse(close, out var closeTime))
            {
                return false;
            }

            return closeTime > openTime;
        }
    }
}
