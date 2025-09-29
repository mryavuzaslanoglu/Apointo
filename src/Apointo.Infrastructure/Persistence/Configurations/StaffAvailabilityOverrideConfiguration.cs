using Apointo.Domain.Staff;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apointo.Infrastructure.Persistence.Configurations;

public sealed class StaffAvailabilityOverrideConfiguration : IEntityTypeConfiguration<StaffAvailabilityOverride>
{
    public void Configure(EntityTypeBuilder<StaffAvailabilityOverride> builder)
    {
        builder.ToTable("StaffAvailabilityOverrides", "dbo");

        builder.Property(o => o.Date)
            .HasColumnType("date")
            .IsRequired();

        builder.Property(o => o.Type)
            .IsRequired();

        builder.Property(o => o.StartTime)
            .HasColumnType("time");

        builder.Property(o => o.EndTime)
            .HasColumnType("time");

        builder.Property(o => o.Reason)
            .HasMaxLength(256);

        builder.HasIndex(o => new { o.StaffId, o.Date, o.Type });
    }
}
