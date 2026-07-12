using Microsoft.EntityFrameworkCore;
using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Infrastructure.Persistence;
using TaskManagement.Infrastructure.Persistence.Seeding;

namespace TaskManagement.Api.Startup;

public static class DatabaseStartupExtensions
{
    /// <summary>
    /// Applies any pending migrations and seeds Development data. Intended to run
    /// only in the Development environment.
    /// </summary>
    public static async Task SeedDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;

        var context = services.GetRequiredService<AppDbContext>();
        await context.Database.MigrateAsync();

        var passwordHasher = services.GetRequiredService<IPasswordHasher>();
        await DatabaseSeeder.SeedAsync(context, passwordHasher);
    }
}
