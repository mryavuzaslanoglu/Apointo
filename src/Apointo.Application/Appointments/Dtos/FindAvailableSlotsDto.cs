using System;
using System.Collections.Generic;

namespace Apointo.Application.Appointments.Dtos;

public sealed record FindAvailableSlotsDto(
    DateTime SearchDate,
    int TotalDurationInMinutes,
    List<AvailableSlotDto> AvailableSlots);