using Apointo.Domain.Businesses;
using Apointo.Domain.Staff;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apointo.Infrastructure.Persistence.Configurations;

public sealed class StaffConfiguration : IEntityTypeConfiguration<Staff>
{
    public void Configure(EntityTypeBuilder<Staff> builder)
    {
        builder.ToTable("Staff", "dbo");

        builder.Property(s => s.FirstName)
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(s => s.LastName)
            .HasMaxLength(128)
            .IsRequired();

        builder.Property(s => s.Email)
            .HasMaxLength(256);

        builder.Property(s => s.PhoneNumber)
            .HasMaxLength(64);

        builder.Property(s => s.Title)
            .HasMaxLength(128);

        builder.Property(s => s.IsActive)
            .HasDefaultValue(true);

        builder.HasIndex(s => new { s.BusinessId, s.Email })
            .HasFilter("[Email] IS NOT NULL")
            .IsUnique();

        builder.HasOne<Business>()
            .WithMany()
            .HasForeignKey(s => s.BusinessId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(s => s.Schedules)
            .WithOne()
            .HasForeignKey(s => s.StaffId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(s => s.AvailabilityOverrides)
            .WithOne()
            .HasForeignKey(o => o.StaffId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(s => s.StaffServices)
            .WithOne()
            .HasForeignKey(ss => ss.StaffId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

