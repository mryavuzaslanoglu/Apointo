using Apointo.Domain.Businesses;
using Apointo.Domain.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apointo.Infrastructure.Persistence.Configurations;

public sealed class ServiceCategoryConfiguration : IEntityTypeConfiguration<ServiceCategory>
{
    public void Configure(EntityTypeBuilder<ServiceCategory> builder)
    {
        builder.ToTable("ServiceCategories", "dbo");

        builder.Property(c => c.Name)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(c => c.Description)
            .HasMaxLength(1024);

        builder.Property(c => c.DisplayOrder)
            .HasDefaultValue(0);

        builder.HasIndex(c => new { c.BusinessId, c.Name })
            .IsUnique();

        builder.HasOne<Business>()
            .WithMany()
            .HasForeignKey(c => c.BusinessId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
