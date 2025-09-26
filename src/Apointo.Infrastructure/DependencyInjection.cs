using System;
using System.Text;
using Apointo.Application.Common.Interfaces;
using Apointo.Application.Common.Interfaces.Authentication;
using Apointo.Application.Common.Interfaces.Identity;
using Apointo.Application.Common.Interfaces.Notifications;
using Apointo.Application.Common.Interfaces.Persistence;
using Apointo.Application.Configuration;
using Apointo.Domain.Identity;
using Apointo.Infrastructure.Identity;
using Apointo.Infrastructure.Persistence;
using Apointo.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Apointo.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = ResolveConnectionString(configuration);
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Connection string 'DefaultConnection' is not configured. " +
                "Set ConnectionStrings__DefaultConnection (or APOINTO_DEFAULT_CONNECTION) via environment variables or user secrets.");
        }

        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
        var jwtSettings = configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
            ?? throw new InvalidOperationException("Jwt settings are missing in configuration.");

        if (string.IsNullOrWhiteSpace(jwtSettings.SigningKey))
        {
            throw new InvalidOperationException(
                "JWT signing key is not configured. Set Jwt:SigningKey securely via environment variables or user secrets.");
        }

        if (Encoding.UTF8.GetByteCount(jwtSettings.SigningKey) < 32)
        {
            throw new InvalidOperationException(
                "JWT signing key must be at least 256 bits (32 bytes) when using HS256. Provide a stronger value via configuration.");
        }

        services.Configure<EmailSettings>(configuration.GetSection(EmailSettings.SectionName));
        var emailSettings = configuration.GetSection(EmailSettings.SectionName).Get<EmailSettings>() ?? new EmailSettings();
        if (emailSettings.IsEnabled)
        {
            if (string.IsNullOrWhiteSpace(emailSettings.FromEmail) || string.IsNullOrWhiteSpace(emailSettings.SmtpHost))
            {
                throw new InvalidOperationException("Email settings are enabled but incomplete. Please configure EmailSettings in secrets or environment variables.");
            }
        }

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());
        services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();
        services.AddSingleton<ITokenService, TokenService>();
        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IEmailSender, SmtpEmailSender>();
        services.AddScoped<ApplicationDbContextInitializer>();

        services.AddHttpContextAccessor();

        var identityBuilder = services
            .AddIdentityCore<ApplicationUser>(ConfigureIdentityOptions);

        identityBuilder = identityBuilder
            .AddRoles<ApplicationRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddSignInManager()
            .AddDefaultTokenProviders();

        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SigningKey)),
                    ValidateIssuer = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwtSettings.Audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
            });

        services.AddAuthorization(options =>
        {
            options.AddPolicy("RequireAdminRole", policy => policy.RequireRole(RoleNames.Admin));
            options.AddPolicy("RequireStaffRole", policy => policy.RequireRole(RoleNames.Admin, RoleNames.Staff));
        });

        return services;
    }

    private static void ConfigureIdentityOptions(IdentityOptions options)
    {
        options.User.RequireUniqueEmail = true;

        options.Password.RequiredLength = 8;
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = true;

        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
        options.Lockout.MaxFailedAccessAttempts = 5;
    }

    private static string? ResolveConnectionString(IConfiguration configuration)
    {
        var fromConfiguration = configuration.GetConnectionString("DefaultConnection");
        if (!string.IsNullOrWhiteSpace(fromConfiguration))
        {
            return fromConfiguration;
        }

        var fromEnvironment = Environment.GetEnvironmentVariable("APOINTO_DEFAULT_CONNECTION");
        if (!string.IsNullOrWhiteSpace(fromEnvironment))
        {
            return fromEnvironment;
        }

        return Environment.GetEnvironmentVariable("SQLCONNSTR_DefaultConnection");
    }
}
