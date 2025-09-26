using Apointo.Domain.Services;
using Apointo.Domain.Staff;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apointo.Infrastructure.Persistence.Configurations;

public sealed class StaffServiceConfiguration : IEntityTypeConfiguration<StaffService>
{
    public void Configure(EntityTypeBuilder<StaffService> builder)
    {
        builder.ToTable("StaffServices", "Core");

        builder.HasIndex(ss => new { ss.StaffId, ss.ServiceId })
            .IsUnique();

        builder.HasOne(ss => ss.Staff)
            .WithMany(s => s.StaffServices)
            .HasForeignKey(ss => ss.StaffId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ss => ss.Service)
            .WithMany(s => s.StaffServices)
            .HasForeignKey(ss => ss.ServiceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
