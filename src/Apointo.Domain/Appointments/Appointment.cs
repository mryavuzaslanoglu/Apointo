using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Apointo.Domain.Common;

namespace Apointo.Domain.Appointments;

public sealed class Appointment : BaseEntity, IAuditableEntity
{
    private readonly List<AppointmentService> _appointmentServices = new();

    private Appointment()
    {
    }

    private Appointment(
        Guid businessId,
        Guid customerId,
        Guid staffId,
        DateTime startTimeUtc,
        DateTime endTimeUtc,
        decimal totalPrice,
        string? notes,
        AppointmentStatus status)
    {
        BusinessId = businessId;
        CustomerId = customerId;
        StaffId = staffId;
        StartTimeUtc = startTimeUtc;
        EndTimeUtc = endTimeUtc;
        TotalPrice = totalPrice;
        Notes = notes;
        Status = status;
    }

    public Guid BusinessId { get; private set; }
    public Guid CustomerId { get; private set; }
    public Guid StaffId { get; private set; }
    public DateTime StartTimeUtc { get; private set; }
    public DateTime EndTimeUtc { get; private set; }
    public decimal TotalPrice { get; private set; }
    public string? Notes { get; private set; }
    public AppointmentStatus Status { get; private set; }
    public string? CancellationReason { get; private set; }
    public DateTime? CancelledAtUtc { get; private set; }
    public Guid? CancelledBy { get; private set; }

    public IReadOnlyCollection<AppointmentService> AppointmentServices =>
        new ReadOnlyCollection<AppointmentService>(_appointmentServices);

    public DateTime CreatedAtUtc { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime? LastModifiedAtUtc { get; set; }
    public Guid? LastModifiedBy { get; set; }

    public int DurationInMinutes => (int)(EndTimeUtc - StartTimeUtc).TotalMinutes;

    public static Appointment Create(
        Guid businessId,
        Guid customerId,
        Guid staffId,
        DateTime startTimeUtc,
        DateTime endTimeUtc,
        decimal totalPrice,
        string? notes = null,
        AppointmentStatus status = AppointmentStatus.Scheduled)
    {
        var appointment = new Appointment(
            businessId,
            customerId,
            staffId,
            startTimeUtc,
            endTimeUtc,
            totalPrice,
            notes,
            status);

        // Raise domain event for appointment creation
        appointment.RaiseDomainEvent(new AppointmentCreatedDomainEvent(appointment.Id));

        return appointment;
    }

    public void AddService(Guid serviceId, decimal price, int durationInMinutes)
    {
        if (_appointmentServices.Any(x => x.ServiceId == serviceId))
        {
            return;
        }

        _appointmentServices.Add(AppointmentService.Create(Id, serviceId, price, durationInMinutes));
    }

    public void RemoveService(Guid serviceId)
    {
        var appointmentService = _appointmentServices.FirstOrDefault(x => x.ServiceId == serviceId);
        if (appointmentService is not null)
        {
            _appointmentServices.Remove(appointmentService);
        }
    }

    public void UpdateSchedule(DateTime startTimeUtc, DateTime endTimeUtc)
    {
        if (Status == AppointmentStatus.Completed || Status == AppointmentStatus.Cancelled)
        {
            throw new InvalidOperationException($"Cannot reschedule appointment with status {Status}");
        }

        var oldStartTime = StartTimeUtc;
        var oldEndTime = EndTimeUtc;

        StartTimeUtc = startTimeUtc;
        EndTimeUtc = endTimeUtc;

        if (Status != AppointmentStatus.Scheduled)
        {
            Status = AppointmentStatus.Rescheduled;
        }

        RaiseDomainEvent(new AppointmentRescheduledDomainEvent(Id, oldStartTime, oldEndTime, startTimeUtc, endTimeUtc));
    }

    public void Confirm()
    {
        if (Status != AppointmentStatus.Scheduled)
        {
            throw new InvalidOperationException($"Cannot confirm appointment with status {Status}");
        }

        Status = AppointmentStatus.Confirmed;
        RaiseDomainEvent(new AppointmentConfirmedDomainEvent(Id));
    }

    public void StartService()
    {
        if (Status != AppointmentStatus.Confirmed && Status != AppointmentStatus.Scheduled)
        {
            throw new InvalidOperationException($"Cannot start appointment with status {Status}");
        }

        Status = AppointmentStatus.InProgress;
        RaiseDomainEvent(new AppointmentStartedDomainEvent(Id));
    }

    public void Complete()
    {
        if (Status != AppointmentStatus.InProgress)
        {
            throw new InvalidOperationException($"Cannot complete appointment with status {Status}");
        }

        Status = AppointmentStatus.Completed;
        RaiseDomainEvent(new AppointmentCompletedDomainEvent(Id));
    }

    public void Cancel(string? reason = null, Guid? cancelledBy = null)
    {
        if (Status == AppointmentStatus.Completed)
        {
            throw new InvalidOperationException("Cannot cancel a completed appointment");
        }

        if (Status == AppointmentStatus.Cancelled)
        {
            return; // Already cancelled
        }

        Status = AppointmentStatus.Cancelled;
        CancellationReason = reason;
        CancelledAtUtc = DateTime.UtcNow;
        CancelledBy = cancelledBy;

        RaiseDomainEvent(new AppointmentCancelledDomainEvent(Id, reason));
    }

    public void MarkAsNoShow()
    {
        if (Status != AppointmentStatus.Scheduled && Status != AppointmentStatus.Confirmed)
        {
            throw new InvalidOperationException($"Cannot mark as no-show appointment with status {Status}");
        }

        Status = AppointmentStatus.NoShow;
        RaiseDomainEvent(new AppointmentNoShowDomainEvent(Id));
    }

    public void UpdateNotes(string? notes)
    {
        Notes = notes;
    }

    public void UpdateTotalPrice(decimal totalPrice)
    {
        if (totalPrice < 0)
        {
            throw new ArgumentException("Total price cannot be negative", nameof(totalPrice));
        }

        TotalPrice = totalPrice;
    }
}