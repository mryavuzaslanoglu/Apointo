using System;
using System.Collections.Generic;

namespace Apointo.Application.Appointments.Dtos;

public sealed record CalendarAppointmentDto(
    string Id,
    string Title,
    DateTime StartTime,
    DateTime EndTime,
    string StaffId,
    string StaffName,
    string CustomerId,
    string CustomerName,
    string Status,
    decimal TotalPrice,
    string? Notes,
    List<string> ServiceNames,
    string? ColorHex);

public sealed record CalendarViewDto(
    DateTime StartDate,
    DateTime EndDate,
    List<CalendarAppointmentDto> Appointments,
    List<StaffCalendarInfoDto> Staff);

public sealed record StaffCalendarInfoDto(
    string Id,
    string Name,
    string? ColorHex);