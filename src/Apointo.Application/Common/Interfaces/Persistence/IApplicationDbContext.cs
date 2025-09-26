using System.Threading;
using System.Threading.Tasks;
using Apointo.Domain.Businesses;
using Apointo.Domain.Identity;
using Apointo.Domain.Services;
using Microsoft.EntityFrameworkCore;

namespace Apointo.Application.Common.Interfaces.Persistence;

public interface IApplicationDbContext
{
    DbSet<Business> Businesses { get; }
    DbSet<Apointo.Domain.Staff.Staff> StaffMembers { get; }
    DbSet<Apointo.Domain.Staff.StaffSchedule> StaffSchedules { get; }
    DbSet<Apointo.Domain.Staff.StaffAvailabilityOverride> StaffAvailabilityOverrides { get; }
    DbSet<ServiceCategory> ServiceCategories { get; }
    DbSet<Service> Services { get; }
    DbSet<Apointo.Domain.Staff.StaffService> StaffServices { get; }
    DbSet<RefreshToken> RefreshTokens { get; }

    DbSet<TEntity> Set<TEntity>() where TEntity : class;
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
