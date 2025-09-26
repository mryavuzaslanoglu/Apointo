using Apointo.Domain.Businesses;
using Apointo.Domain.Businesses.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apointo.Infrastructure.Persistence.Configurations;

public sealed class BusinessConfiguration : IEntityTypeConfiguration<Business>
{
    public void Configure(EntityTypeBuilder<Business> builder)
    {
        builder.ToTable("Businesses", "Core");

        builder.Property(b => b.Name)
            .HasMaxLength(256)
            .IsRequired();

        builder.Property(b => b.Description)
            .HasMaxLength(1024);

        builder.Property(b => b.PhoneNumber)
            .HasMaxLength(64);

        builder.Property(b => b.Email)
            .HasMaxLength(256);

        builder.Property(b => b.WebsiteUrl)
            .HasMaxLength(512);

        builder.OwnsOne(b => b.Address, addressBuilder =>
        {
            addressBuilder.Property(a => a.Line1)
                .HasColumnName("AddressLine1")
                .HasMaxLength(256);

            addressBuilder.Property(a => a.Line2)
                .HasColumnName("AddressLine2")
                .HasMaxLength(256);

            addressBuilder.Property(a => a.City)
                .HasColumnName("AddressCity")
                .HasMaxLength(128);

            addressBuilder.Property(a => a.State)
                .HasColumnName("AddressState")
                .HasMaxLength(128);

            addressBuilder.Property(a => a.PostalCode)
                .HasColumnName("AddressPostalCode")
                .HasMaxLength(32);

            addressBuilder.Property(a => a.Country)
                .HasColumnName("AddressCountry")
                .HasMaxLength(128);
        });

        builder.OwnsMany(b => b.OperatingHours, hoursBuilder =>
        {
            hoursBuilder.ToTable("BusinessOperatingHours", "Core");
            hoursBuilder.WithOwner().HasForeignKey("BusinessId");

            hoursBuilder.Property<int>("Id");
            hoursBuilder.HasKey("Id");

            hoursBuilder.Property(h => h.DayOfWeek)
                .IsRequired();

            hoursBuilder.Property(h => h.IsClosed)
                .IsRequired();

            hoursBuilder.Property(h => h.OpenTime)
                .HasColumnType("time");

            hoursBuilder.Property(h => h.CloseTime)
                .HasColumnType("time");

            hoursBuilder.HasIndex("BusinessId", nameof(BusinessOperatingHour.DayOfWeek)).IsUnique();
        });
    }
}
