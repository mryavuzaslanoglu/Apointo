using System;
using System.Threading;
using System.Threading.Tasks;
using Apointo.Application.Common.Interfaces;
using Apointo.Application.Common.Interfaces.Persistence;
using Apointo.Domain.Appointments;
using Apointo.Domain.Businesses;
using Apointo.Domain.Common;
using Apointo.Domain.Identity;
using Apointo.Domain.Services;
using Apointo.Domain.Staff;
using Apointo.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Apointo.Infrastructure.Persistence;

public sealed class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>, IApplicationDbContext
{
    private readonly IDateTimeProvider _dateTimeProvider;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        IDateTimeProvider dateTimeProvider) : base(options)
    {
        _dateTimeProvider = dateTimeProvider;
    }

    public DbSet<Business> Businesses => Set<Business>();
    public DbSet<Staff> StaffMembers => Set<Staff>();
    public DbSet<StaffSchedule> StaffSchedules => Set<StaffSchedule>();
    public DbSet<StaffAvailabilityOverride> StaffAvailabilityOverrides => Set<StaffAvailabilityOverride>();
    public DbSet<ServiceCategory> ServiceCategories => Set<ServiceCategory>();
    public DbSet<Service> Services => Set<Service>();
    public DbSet<StaffService> StaffServices => Set<StaffService>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<AppointmentService> AppointmentServices => Set<AppointmentService>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Tüm tablolar dbo schema'sında olsun
        builder.HasDefaultSchema("dbo");

        builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<IAuditableEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Property(nameof(IAuditableEntity.CreatedAtUtc)).CurrentValue = _dateTimeProvider.UtcNow;
                    break;
                case EntityState.Modified:
                    entry.Property(nameof(IAuditableEntity.LastModifiedAtUtc)).CurrentValue = _dateTimeProvider.UtcNow;
                    break;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}
