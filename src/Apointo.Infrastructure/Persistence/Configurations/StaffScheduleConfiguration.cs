using Apointo.Domain.Staff;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apointo.Infrastructure.Persistence.Configurations;

public sealed class StaffScheduleConfiguration : IEntityTypeConfiguration<StaffSchedule>
{
    public void Configure(EntityTypeBuilder<StaffSchedule> builder)
    {
        builder.ToTable("StaffSchedules", "Core");

        builder.Property(s => s.DayOfWeek)
            .IsRequired();

        builder.Property(s => s.IsWorking)
            .IsRequired();

        builder.Property(s => s.StartTime)
            .HasColumnType("time");

        builder.Property(s => s.EndTime)
            .HasColumnType("time");

        builder.HasIndex(s => new { s.StaffId, s.DayOfWeek })
            .IsUnique();
    }
}
