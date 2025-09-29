using Apointo.Domain.Appointments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apointo.Infrastructure.Persistence.Configurations;

public sealed class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
{
    public void Configure(EntityTypeBuilder<Appointment> builder)
    {
        builder.ToTable("Appointments", "dbo");

        builder.Property(a => a.Id)
            .IsRequired();

        builder.Property(a => a.BusinessId)
            .IsRequired();

        builder.Property(a => a.CustomerId)
            .IsRequired();

        builder.Property(a => a.StaffId)
            .IsRequired();

        builder.Property(a => a.StartTimeUtc)
            .IsRequired();

        builder.Property(a => a.EndTimeUtc)
            .IsRequired();

        builder.Property(a => a.TotalPrice)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(a => a.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(a => a.Notes)
            .HasMaxLength(1000);

        builder.Property(a => a.CancellationReason)
            .HasMaxLength(500);

        builder.Property(a => a.CancelledAtUtc);

        builder.Property(a => a.CancelledBy);

        // Indexes for performance
        builder.HasIndex(a => a.CustomerId);
        builder.HasIndex(a => a.StaffId);
        builder.HasIndex(a => a.BusinessId);
        builder.HasIndex(a => a.StartTimeUtc);
        builder.HasIndex(a => new { a.StaffId, a.StartTimeUtc, a.EndTimeUtc });

        // Relationships
        builder.HasMany(a => a.AppointmentServices)
            .WithOne(aps => aps.Appointment)
            .HasForeignKey(aps => aps.AppointmentId)
            .OnDelete(DeleteBehavior.Cascade);

        // Ignore DomainEvents collection
        builder.Ignore(a => a.DomainEvents);
    }
}