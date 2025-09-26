using Apointo.Domain.Identity;

namespace Apointo.Application.Common.Interfaces.Identity;

public interface IRefreshTokenRepository
{
    Task AddAsync(RefreshToken refreshToken, CancellationToken cancellationToken);
    Task<IReadOnlyList<RefreshToken>> GetActiveTokensByUserIdAsync(Guid userId, CancellationToken cancellationToken);
    Task<int> RemoveExpiredTokensAsync(Guid userId, DateTime utcNow, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}