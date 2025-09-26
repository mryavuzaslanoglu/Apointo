using System;
using System.Linq;
using Apointo.Application.Common.Interfaces.Identity;
using Apointo.Domain.Identity;
using Apointo.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Apointo.Infrastructure.Identity;

public sealed class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly ApplicationDbContext _context;

    public RefreshTokenRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken)
    {
        await _context.RefreshTokens.AddAsync(refreshToken, cancellationToken);
    }

    public async Task<IReadOnlyList<RefreshToken>> GetActiveTokensByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await _context.RefreshTokens
            .Where(x => x.UserId == userId && !x.IsRevoked)
            .ToListAsync(cancellationToken);
    }

    public Task<int> RemoveExpiredTokensAsync(Guid userId, DateTime utcNow, CancellationToken cancellationToken)
    {
        return _context.RefreshTokens
            .Where(x => x.UserId == userId && (x.IsRevoked || x.ExpiresAtUtc <= utcNow))
            .ExecuteDeleteAsync(cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }
}