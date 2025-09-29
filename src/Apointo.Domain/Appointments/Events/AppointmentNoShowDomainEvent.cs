using System;
using Apointo.Domain.Common;

namespace Apointo.Domain.Appointments;

public sealed record AppointmentNoShowDomainEvent(Guid AppointmentId) : IDomainEvent;