using System;
using Apointo.Domain.Common;

namespace Apointo.Domain.Appointments;

public sealed class AppointmentService : BaseEntity
{
    private AppointmentService()
    {
    }

    private AppointmentService(
        Guid appointmentId,
        Guid serviceId,
        decimal price,
        int durationInMinutes)
    {
        AppointmentId = appointmentId;
        ServiceId = serviceId;
        Price = price;
        DurationInMinutes = durationInMinutes;
    }

    public Guid AppointmentId { get; private set; }
    public Guid ServiceId { get; private set; }
    public decimal Price { get; private set; }
    public int DurationInMinutes { get; private set; }

    // Navigation properties
    public Appointment? Appointment { get; private set; }
    public Services.Service? Service { get; private set; }

    public static AppointmentService Create(
        Guid appointmentId,
        Guid serviceId,
        decimal price,
        int durationInMinutes)
    {
        return new AppointmentService(appointmentId, serviceId, price, durationInMinutes);
    }

    public void UpdatePrice(decimal price)
    {
        if (price < 0)
        {
            throw new ArgumentException("Price cannot be negative", nameof(price));
        }

        Price = price;
    }

    public void UpdateDuration(int durationInMinutes)
    {
        if (durationInMinutes <= 0)
        {
            throw new ArgumentException("Duration must be positive", nameof(durationInMinutes));
        }

        DurationInMinutes = durationInMinutes;
    }
}