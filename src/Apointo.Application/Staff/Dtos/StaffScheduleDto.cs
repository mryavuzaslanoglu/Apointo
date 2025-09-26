using System;

namespace Apointo.Application.Staff.Dtos;

public sealed record StaffScheduleDto(
    DayOfWeek DayOfWeek,
    bool IsWorking,
    string? StartTime,
    string? EndTime);
