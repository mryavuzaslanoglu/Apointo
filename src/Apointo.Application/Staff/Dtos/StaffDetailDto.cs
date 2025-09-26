using System;
using System.Collections.Generic;

namespace Apointo.Application.Staff.Dtos;

public sealed record StaffDetailDto(
    string Id,
    string FirstName,
    string LastName,
    string FullName,
    string? Title,
    string? Email,
    string? PhoneNumber,
    bool IsActive,
    string? UserId,
    DateTime? HiredAtUtc,
    DateTime? TerminatedAtUtc,
    IReadOnlyCollection<StaffScheduleDto> Schedules,
    IReadOnlyCollection<StaffAvailabilityOverrideDto> AvailabilityOverrides);
