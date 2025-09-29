using Apointo.Domain.Appointments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apointo.Infrastructure.Persistence.Configurations;

public sealed class AppointmentServiceConfiguration : IEntityTypeConfiguration<AppointmentService>
{
    public void Configure(EntityTypeBuilder<AppointmentService> builder)
    {
        builder.ToTable("AppointmentServices", "dbo");

        builder.Property(aps => aps.Id)
            .IsRequired();

        builder.Property(aps => aps.AppointmentId)
            .IsRequired();

        builder.Property(aps => aps.ServiceId)
            .IsRequired();

        builder.Property(aps => aps.Price)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(aps => aps.DurationInMinutes)
            .IsRequired();

        // Indexes
        builder.HasIndex(aps => aps.AppointmentId);
        builder.HasIndex(aps => aps.ServiceId);
        builder.HasIndex(aps => new { aps.AppointmentId, aps.ServiceId })
            .IsUnique();

        // Relationships
        builder.HasOne(aps => aps.Service)
            .WithMany()
            .HasForeignKey(aps => aps.ServiceId)
            .OnDelete(DeleteBehavior.Restrict);

        // Ignore DomainEvents collection
        builder.Ignore(aps => aps.DomainEvents);
    }
}