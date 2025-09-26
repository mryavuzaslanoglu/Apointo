using System;

namespace Apointo.Application.Businesses.Dtos;

public sealed record BusinessOperatingHourDto(
    DayOfWeek DayOfWeek,
    bool IsClosed,
    string? OpenTime,
    string? CloseTime);
