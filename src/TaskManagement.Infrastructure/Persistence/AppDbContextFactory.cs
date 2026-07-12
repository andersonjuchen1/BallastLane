using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace TaskManagement.Infrastructure.Persistence;

/// <summary>
/// Design-time factory used by the EF Core CLI (migrations add / database
/// update). It is never used at runtime — the app configures the DbContext via
/// DI. The connection string here targets the local dev database only (Windows
/// auth, no secret), so it is safe to keep in source.
/// </summary>
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    private const string DesignTimeConnectionString =
        "Server=localhost;Database=TaskManagementDb;Trusted_Connection=True;TrustServerCertificate=True;";

    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseSqlServer(DesignTimeConnectionString);

        return new AppDbContext(optionsBuilder.Options);
    }
}
