using System;
using System.IO;
using Apointo.Application.Common.Interfaces;
using Apointo.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Apointo.Infrastructure.Persistence;

public sealed class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var currentDirectory = Directory.GetCurrentDirectory();
        var potentialBasePaths = new[]
        {
            currentDirectory,
            Path.Combine(currentDirectory, "src", "Apointo.Api"),
            Path.Combine(currentDirectory, "..", "Apointo.Api"),
            Path.Combine(currentDirectory, "..", "Apointo.Api", "..", "Apointo.Api")
        };

        string? basePath = null;
        foreach (var candidate in potentialBasePaths)
        {
            if (File.Exists(Path.Combine(candidate, "appsettings.json")))
            {
                basePath = candidate;
                break;
            }
        }

        basePath ??= currentDirectory;

        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = ResolveConnectionString(configuration);
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "Connection string 'DefaultConnection' is not configured. " +
                "Set ConnectionStrings__DefaultConnection (or APOINTO_DEFAULT_CONNECTION) via environment variables or user secrets.");
        }

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(connectionString);

        IDateTimeProvider dateTimeProvider = new SystemDateTimeProvider();

        return new ApplicationDbContext(optionsBuilder.Options, dateTimeProvider);
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

        // Azure style environment variable convention
        return Environment.GetEnvironmentVariable("SQLCONNSTR_DefaultConnection");
    }
}