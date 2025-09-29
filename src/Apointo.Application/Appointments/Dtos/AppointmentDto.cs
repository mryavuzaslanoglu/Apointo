using System;
using System.Collections.Generic;

namespace Apointo.Application.Appointments.Dtos;

public sealed record AppointmentDto(
    string Id,
    string CustomerId,
    string StaffId,
    string StaffName,
    DateTime StartTime,
    DateTime EndTime,
    decimal TotalPrice,
    string Status,
    string? Notes,
    List<AppointmentServiceDto> Services);

public sealed record AppointmentServiceDto(
    string ServiceId,
    string ServiceName,
    decimal Price,
    int DurationInMinutes);