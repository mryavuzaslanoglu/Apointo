using Apointo.Domain.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apointo.Infrastructure.Persistence.Configurations;

public sealed class ServiceConfiguration : IEntityTypeConfiguration<Service>
{
    public void Configure(EntityTypeBuilder<Service> builder)
    {
        builder.ToTable("Services", "dbo");

        builder.Property(s => s.Name)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(s => s.Description)
            .HasMaxLength(1024);

        builder.Property(s => s.Price)
            .HasPrecision(18, 2);

        builder.Property(s => s.ColorHex)
            .HasMaxLength(16);

        builder.Property(s => s.DurationInMinutes)
            .IsRequired();

        builder.Property(s => s.BufferTimeInMinutes)
            .IsRequired();

        builder.HasIndex(s => new { s.BusinessId, s.Name })
            .IsUnique();

        builder.HasOne(s => s.Category)
            .WithMany(c => c.Services)
            .HasForeignKey(s => s.ServiceCategoryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(s => s.StaffServices)
            .WithOne()
            .HasForeignKey(ss => ss.ServiceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

