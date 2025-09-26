using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Apointo.Domain.Businesses;
using Apointo.Domain.Businesses.ValueObjects;
using Apointo.Domain.Identity;
using Apointo.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Apointo.Infrastructure.Persistence;

public sealed class ApplicationDbContextInitializer
{
    private static readonly (string Email, string Password, string FirstName, string LastName, string Role)[] DefaultUsers =
    {
        ("admin@apointo.dev", "Admin!123", "System", "Admin", RoleNames.Admin),
        ("staff@apointo.dev", "Staff!123", "Default", "Staff", RoleNames.Staff),
        ("customer@apointo.dev", "Customer!123", "Default", "Customer", RoleNames.Customer)
    };

    private const string DefaultBusinessName = "Apointo Varsayılan İşletme";
    private const string DefaultBusinessDescription = "Seed edilmiş varsayılan işletme kaydı.";
    private const string DefaultBusinessPhone = "+90 212 000 00 00";
    private const string DefaultBusinessEmail = "info@apointo.dev";
    private const string DefaultBusinessWebsite = "https://apointo.dev";

    private readonly ApplicationDbContext _context;
    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<ApplicationDbContextInitializer> _logger;

    public ApplicationDbContextInitializer(
        ApplicationDbContext context,
        RoleManager<ApplicationRole> roleManager,
        UserManager<ApplicationUser> userManager,
        ILogger<ApplicationDbContextInitializer> logger)
    {
        _context = context;
        _roleManager = roleManager;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task InitialiseAsync(CancellationToken cancellationToken = default)
    {
        await _context.Database.MigrateAsync(cancellationToken);
    }

    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        await EnsureRolesAsync();
        await EnsureDefaultUsersAsync();
        await EnsureDefaultBusinessAsync(cancellationToken);
    }

    private async Task EnsureRolesAsync()
    {
        foreach (var role in RoleNames.All)
        {
            if (await _roleManager.RoleExistsAsync(role))
            {
                continue;
            }

            var identityRole = new ApplicationRole(role);
            var result = await _roleManager.CreateAsync(identityRole);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException($"Failed to create role {role}: {string.Join(';', result.Errors.Select(e => e.Description))}");
            }
        }
    }

    private async Task EnsureDefaultUsersAsync()
    {
        foreach (var (email, password, firstName, lastName, role) in DefaultUsers)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user is not null)
            {
                continue;
            }

            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                EmailConfirmed = true,
                IsActive = true
            };

            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                throw new InvalidOperationException($"Failed to create seed user {email}: {string.Join(';', result.Errors.Select(e => e.Description))}");
            }

            await _userManager.AddToRoleAsync(user, role);
            _logger.LogInformation("Seed user created with email {Email} and role {Role}", email, role);
        }
    }

    private async Task EnsureDefaultBusinessAsync(CancellationToken cancellationToken)
    {
        if (await _context.Businesses.AnyAsync(cancellationToken))
        {
            return;
        }

        var defaultHours = Enum.GetValues<DayOfWeek>()
            .Select(day =>
            {
                var isClosed = day == DayOfWeek.Sunday;
                var openTime = isClosed ? (TimeSpan?)null : new TimeSpan(9, 0, 0);
                var closeTime = isClosed ? (TimeSpan?)null : new TimeSpan(18, 0, 0);
                return BusinessOperatingHour.Create(day, isClosed, openTime, closeTime);
            })
            .ToList();

        var business = Business.Create(
            DefaultBusinessName,
            DefaultBusinessDescription,
            DefaultBusinessPhone,
            DefaultBusinessEmail,
            DefaultBusinessWebsite,
            null,
            defaultHours);

        await _context.Businesses.AddAsync(business, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Seed business entity created with name {Name}", DefaultBusinessName);
    }
}
