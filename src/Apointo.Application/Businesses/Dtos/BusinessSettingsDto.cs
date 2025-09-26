using System.Collections.Generic;

namespace Apointo.Application.Businesses.Dtos;

public sealed record BusinessSettingsDto(
    string Id,
    string Name,
    string? Description,
    string? PhoneNumber,
    string? Email,
    string? WebsiteUrl,
    BusinessAddressDto? Address,
    IReadOnlyCollection<BusinessOperatingHourDto> OperatingHours);
