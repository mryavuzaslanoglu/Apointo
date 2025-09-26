using System;
using System.Collections.Generic;

namespace Apointo.Api.Contracts.BusinessSettings;

public sealed record BusinessSettingsResponse(
    string Id,
    string Name,
    string? Description,
    string? PhoneNumber,
    string? Email,
    string? WebsiteUrl,
    BusinessAddressResponse? Address,
    IReadOnlyCollection<BusinessOperatingHourResponse> OperatingHours);

public sealed record BusinessAddressResponse(
    string? Line1,
    string? Line2,
    string? City,
    string? State,
    string? PostalCode,
    string? Country);

public sealed record BusinessOperatingHourResponse(
    DayOfWeek DayOfWeek,
    bool IsClosed,
    string? OpenTime,
    string? CloseTime);

public sealed record UpdateBusinessSettingsRequest(
    string Name,
    string? Description,
    string? PhoneNumber,
    string? Email,
    string? WebsiteUrl,
    BusinessAddressRequest? Address,
    IReadOnlyCollection<BusinessOperatingHourRequest> OperatingHours);

public sealed record BusinessAddressRequest(
    string? Line1,
    string? Line2,
    string? City,
    string? State,
    string? PostalCode,
    string? Country);

public sealed record BusinessOperatingHourRequest(
    DayOfWeek DayOfWeek,
    bool IsClosed,
    string? OpenTime,
    string? CloseTime);
