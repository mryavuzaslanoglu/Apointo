using Apointo.Infrastructure.Persistence;

namespace Apointo.Api.Extensions;

public static class WebApplicationExtensions
{
    public static async Task InitialiseDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var initializer = scope.ServiceProvider.GetRequiredService<ApplicationDbContextInitializer>();
        await initializer.InitialiseAsync();
        await initializer.SeedAsync();
    }
}