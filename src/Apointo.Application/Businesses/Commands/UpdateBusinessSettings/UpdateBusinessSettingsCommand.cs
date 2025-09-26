using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Apointo.Application.Businesses.Dtos;
using Apointo.Application.Common.Interfaces.Persistence;
using Apointo.Application.Common.Models;
using Apointo.Domain.Businesses.ValueObjects;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Apointo.Application.Businesses.Commands.UpdateBusinessSettings;

public sealed record UpdateBusinessSettingsCommand(
    string Name,
    string? Description,
    string? PhoneNumber,
    string? Email,
    string? WebsiteUrl,
    BusinessAddressDto? Address,
    IReadOnlyCollection<BusinessOperatingHourInput> OperatingHours) : IRequest<Result<BusinessSettingsDto>>;

public sealed record BusinessOperatingHourInput(
    DayOfWeek DayOfWeek,
    bool IsClosed,
    string? OpenTime,
    string? CloseTime);

public sealed class UpdateBusinessSettingsCommandHandler : IRequestHandler<UpdateBusinessSettingsCommand, Result<BusinessSettingsDto>>
{
    private static readonly string[] TimeFormats = { @"hh\:mm", @"h\:mm" };
    private readonly IApplicationDbContext _context;

    public UpdateBusinessSettingsCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result<BusinessSettingsDto>> Handle(UpdateBusinessSettingsCommand request, CancellationToken cancellationToken)
    {
        var business = await _context.Businesses
            .Include(b => b.OperatingHours)
            .FirstOrDefaultAsync(cancellationToken);

        if (business is null)
        {
            return Result<BusinessSettingsDto>.Failure("BusinessNotFound");
        }

        Address? address = null;
        if (request.Address is not null)
        {
            var (line1, line2, city, state, postalCode, country) = request.Address;
            var hasAddressData = !string.IsNullOrWhiteSpace(line1) ||
                                 !string.IsNullOrWhiteSpace(line2) ||
                                 !string.IsNullOrWhiteSpace(city) ||
                                 !string.IsNullOrWhiteSpace(state) ||
                                 !string.IsNullOrWhiteSpace(postalCode) ||
                                 !string.IsNullOrWhiteSpace(country);

            if (hasAddressData)
            {
                address = Address.Create(
                    line1 ?? string.Empty,
                    line2,
                    city ?? string.Empty,
                    state,
                    postalCode ?? string.Empty,
                    country ?? string.Empty);
            }
        }

        var operatingHours = request.OperatingHours
            .Select(hour => MapOperatingHour(hour))
            .ToList();

        business.Update(
            request.Name,
            request.Description,
            request.PhoneNumber,
            request.Email,
            request.WebsiteUrl,
            address,
            operatingHours);

        await _context.SaveChangesAsync(cancellationToken);

        var result = new BusinessSettingsDto(
            business.Id.ToString(),
            business.Name,
            business.Description,
            business.PhoneNumber,
            business.Email,
            business.WebsiteUrl,
            business.Address is null
                ? null
                : new BusinessAddressDto(
                    business.Address.Line1,
                    business.Address.Line2,
                    business.Address.City,
                    business.Address.State,
                    business.Address.PostalCode,
                    business.Address.Country),
            business.OperatingHours
                .OrderBy(h => h.DayOfWeek)
                .Select(h => new BusinessOperatingHourDto(
                    h.DayOfWeek,
                    h.IsClosed,
                    h.OpenTime?.ToString(@"hh\:mm"),
                    h.CloseTime?.ToString(@"hh\:mm")))
                .ToList());

        return Result<BusinessSettingsDto>.Success(result);
    }

    private static BusinessOperatingHour MapOperatingHour(BusinessOperatingHourInput input)
    {
        if (input.IsClosed)
        {
            return BusinessOperatingHour.Create(input.DayOfWeek, true, null, null);
        }

        var openTime = ParseTime(input.OpenTime);
        var closeTime = ParseTime(input.CloseTime);

        return BusinessOperatingHour.Create(input.DayOfWeek, false, openTime, closeTime);
    }

    private static TimeSpan ParseTime(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException("Time value is required for open days.");
        }

        if (TimeSpan.TryParseExact(value, TimeFormats, CultureInfo.InvariantCulture, out var time))
        {
            return time;
        }

        throw new InvalidOperationException($"Invalid time format: {value}");
    }
}
