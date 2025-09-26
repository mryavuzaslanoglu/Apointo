using System;
using Apointo.Domain.Common;

namespace Apointo.Domain.Identity;

public sealed class RefreshToken : BaseEntity, IAuditableEntity
{
    public Guid UserId { get; private set; }
    public string TokenHash { get; private set; } = string.Empty;
    public string TokenSalt { get; private set; } = string.Empty;
    public DateTime ExpiresAtUtc { get; private set; }
    public DateTime CreatedAtUtc { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime? LastModifiedAtUtc { get; set; }
    public Guid? LastModifiedBy { get; set; }
    public string? Device { get; private set; }
    public string? IpAddress { get; private set; }
    public bool IsRevoked { get; private set; }
    public DateTime? RevokedAtUtc { get; private set; }
    public string? ReplacedByTokenHash { get; private set; }

    public static RefreshToken Create(
        Guid userId,
        string tokenHash,
        string tokenSalt,
        DateTime createdAtUtc,
        DateTime expiresAtUtc,
        string? device,
        string? ipAddress)
    {
        return new RefreshToken
        {
            UserId = userId,
            TokenHash = tokenHash,
            TokenSalt = tokenSalt,
            CreatedAtUtc = createdAtUtc,
            ExpiresAtUtc = expiresAtUtc,
            Device = device,
            IpAddress = ipAddress
        };
    }

    public void Revoke(DateTime revokedAtUtc, string? replacedByTokenHash = null)
    {
        IsRevoked = true;
        RevokedAtUtc = revokedAtUtc;
        ReplacedByTokenHash = replacedByTokenHash;
    }

    public bool IsActive(DateTime utcNow) => !IsRevoked && ExpiresAtUtc > utcNow;
}