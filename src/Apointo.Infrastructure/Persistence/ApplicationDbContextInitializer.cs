using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Apointo.Domain.Businesses;
using Apointo.Domain.Businesses.ValueObjects;
using Apointo.Domain.Identity;
using Apointo.Domain.Services;
using Apointo.Domain.Staff;
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
        ("customer@apointo.dev", "Customer!123", "Default", "Customer", RoleNames.Customer),
        ("mehmet@gmail.com", "MehmetYilmaz!123", "Mehmet", "Yılmaz", RoleNames.Customer),
        ("ayse@gmail.com", "AyseKaya!123", "Ayşe", "Kaya", RoleNames.Customer),
        ("ali@gmail.com", "AliDemir!123", "Ali", "Demir", RoleNames.Customer)
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
        var business = await EnsureDefaultBusinessAsync(cancellationToken);
        await EnsureServiceCategoriesAsync(business.Id, cancellationToken);
        await EnsureServicesAsync(business.Id, cancellationToken);
        await EnsureStaffAsync(business.Id, cancellationToken);
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

    private async Task<Business> EnsureDefaultBusinessAsync(CancellationToken cancellationToken)
    {
        var existingBusiness = await _context.Businesses.FirstOrDefaultAsync(cancellationToken);
        if (existingBusiness is not null)
        {
            return existingBusiness;
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
        return business;
    }

    private async Task EnsureServiceCategoriesAsync(Guid businessId, CancellationToken cancellationToken)
    {
        if (await _context.ServiceCategories.AnyAsync(cancellationToken))
        {
            return;
        }

        var categories = new[]
        {
            ServiceCategory.Create(businessId, "Saç Bakımı", "Saç kesimi, şekillendirme ve bakım hizmetleri", 1),
            ServiceCategory.Create(businessId, "Sakal & Bıyık", "Sakal kesimi, şekillendirme ve bakım hizmetleri", 2),
            ServiceCategory.Create(businessId, "Cilt Bakımı", "Yüz bakımı ve cilt temizlik hizmetleri", 3)
        };

        await _context.ServiceCategories.AddRangeAsync(categories, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Seed service categories created: {Count} categories", categories.Length);
    }

    private async Task EnsureServicesAsync(Guid businessId, CancellationToken cancellationToken)
    {
        if (await _context.Services.AnyAsync(cancellationToken))
        {
            return;
        }

        var categories = await _context.ServiceCategories.ToListAsync(cancellationToken);
        var hairCategory = categories.First(c => c.Name == "Saç Bakımı");
        var beardCategory = categories.First(c => c.Name == "Sakal & Bıyık");
        var skinCategory = categories.First(c => c.Name == "Cilt Bakımı");

        var services = new[]
        {
            Service.Create(businessId, hairCategory.Id, "Erkek Saç Kesimi", "Klasik erkek saç kesimi", 50.00m, 30, 5, true, "#FF6B6B"),
            Service.Create(businessId, hairCategory.Id, "Saç Yıkama & Fön", "Şampuan, saç yıkama ve fön", 25.00m, 20, 5, true, "#4ECDC4"),
            Service.Create(businessId, hairCategory.Id, "Saç Boyama", "Profesyonel saç boyama hizmeti", 120.00m, 90, 10, true, "#45B7D1"),
            Service.Create(businessId, beardCategory.Id, "Sakal Kesimi", "Profesyonel sakal kesimi ve şekillendirme", 30.00m, 20, 5, true, "#96CEB4"),
            Service.Create(businessId, beardCategory.Id, "Bıyık Şekillendirme", "Bıyık kesimi ve şekillendirme", 15.00m, 10, 5, true, "#FECA57"),
            Service.Create(businessId, beardCategory.Id, "Sakal Bakımı", "Sakal yağı ve bakım ürünleri ile bakım", 40.00m, 25, 5, true, "#48CAE4"),
            Service.Create(businessId, skinCategory.Id, "Yüz Temizliği", "Derin yüz temizliği ve peeling", 80.00m, 45, 10, true, "#FF9FF3"),
            Service.Create(businessId, skinCategory.Id, "Yüz Maskesi", "Nemlendirici ve rahatlatıcı yüz maskesi", 60.00m, 30, 5, true, "#F38BA8")
        };

        await _context.Services.AddRangeAsync(services, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Seed services created: {Count} services", services.Length);
    }

    private async Task EnsureStaffAsync(Guid businessId, CancellationToken cancellationToken)
    {
        if (await _context.StaffMembers.AnyAsync(cancellationToken))
        {
            return;
        }

        var staff = new[]
        {
            Staff.Create(businessId, "Ahmet", "Yılmaz", "ahmet@apointo.dev", "+90 532 111 11 11", "Baş Berber", null, DateTime.UtcNow.AddMonths(-12)),
            Staff.Create(businessId, "Mustafa", "Kaya", "mustafa@apointo.dev", "+90 532 222 22 22", "Berber", null, DateTime.UtcNow.AddMonths(-6)),
            Staff.Create(businessId, "Oğuz", "Demir", "oguz@apointo.dev", "+90 532 333 33 33", "Berber", null, DateTime.UtcNow.AddMonths(-3))
        };

        await _context.StaffMembers.AddRangeAsync(staff, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Seed staff created: {Count} staff members", staff.Length);
    }
}
