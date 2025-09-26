using Apointo.Domain.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Apointo.Infrastructure.Persistence.Configurations;

public sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.TokenHash)
            .IsRequired()
            .HasMaxLength(512);

        builder.Property(x => x.TokenSalt)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(x => x.Device)
            .HasMaxLength(256);

        builder.Property(x => x.IpAddress)
            .HasMaxLength(64);

        builder.Property(x => x.ExpiresAtUtc)
            .IsRequired();

        builder.HasIndex(x => new { x.UserId, x.TokenHash })
            .IsUnique();
    }
}