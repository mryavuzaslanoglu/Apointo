using System;
using Apointo.Domain.Staff;

namespace Apointo.Application.Staff.Dtos;

public sealed record StaffAvailabilityOverrideDto(
    string Id,
    DateOnly Date,
    StaffAvailabilityType Type,
    string? StartTime,
    string? EndTime,
    string? Reason);
