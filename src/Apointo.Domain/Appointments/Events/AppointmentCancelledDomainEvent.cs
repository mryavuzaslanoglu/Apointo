using System;
using Apointo.Domain.Common;

namespace Apointo.Domain.Appointments;

public sealed record AppointmentCancelledDomainEvent(
    Guid AppointmentId,
    string? CancellationReason) : IDomainEvent;