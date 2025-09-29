using System;

namespace Apointo.Application.Appointments.Dtos;

public sealed record AvailableSlotDto(
    DateTime StartTime,
    DateTime EndTime,
    Guid StaffId,
    string StaffName,
    bool IsAvailable);