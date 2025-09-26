using System;
using System.Collections.Generic;
using Apointo.Domain.Staff;

namespace Apointo.Api.Contracts.Staff;

public sealed record StaffSummaryResponse(
    string Id,
    string FullName,
    string FirstName,
    string LastName,
    string? Title,
    string? Email,
    string? PhoneNumber,
    bool IsActive);

public sealed record StaffResponse(
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
    IReadOnlyCollection<StaffScheduleResponse> Schedules,
    IReadOnlyCollection<StaffAvailabilityOverrideResponse> AvailabilityOverrides);

public sealed record StaffScheduleResponse(
    DayOfWeek DayOfWeek,
    bool IsWorking,
    string? StartTime,
    string? EndTime);

public sealed record StaffAvailabilityOverrideResponse(
    string Id,
    DateOnly Date,
    StaffAvailabilityType Type,
    string? StartTime,
    string? EndTime,
    string? Reason);

public sealed record CreateStaffRequest(
    string FirstName,
    string LastName,
    string? Title,
    string? Email,
    string? PhoneNumber,
    bool IsActive,
    string? UserId,
    DateTime? HiredAtUtc);

public sealed record UpdateStaffRequest(
    string FirstName,
    string LastName,
    string? Title,
    string? Email,
    string? PhoneNumber,
    bool IsActive,
    string? UserId,
    DateTime? HiredAtUtc,
    DateTime? TerminatedAtUtc);

public sealed record UpdateStaffScheduleRequest(
    IReadOnlyCollection<StaffScheduleItemRequest> Schedules);

public sealed record StaffScheduleItemRequest(
    DayOfWeek DayOfWeek,
    bool IsWorking,
    string? StartTime,
    string? EndTime);

public sealed record CreateStaffAvailabilityOverrideRequest(
    DateOnly Date,
    StaffAvailabilityType Type,
    string? StartTime,
    string? EndTime,
    string? Reason);
