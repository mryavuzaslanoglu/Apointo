using System;
using Apointo.Domain.Common;

namespace Apointo.Domain.Appointments;

public sealed record AppointmentRescheduledDomainEvent(
    Guid AppointmentId,
    DateTime OldStartTimeUtc,
    DateTime OldEndTimeUtc,
    DateTime NewStartTimeUtc,
    DateTime NewEndTimeUtc) : IDomainEvent;