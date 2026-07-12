using Microsoft.EntityFrameworkCore;
using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Enums;

namespace TaskManagement.Infrastructure.Persistence.Seeding;

/// <summary>
/// Seeds a demo user and sample tasks for Development. Idempotent: does nothing
/// if any user already exists.
/// </summary>
public static class DatabaseSeeder
{
    public const string DemoUsername = "demo";
    public const string DemoEmail = "demo@example.com";
    public const string DemoPassword = "Passw0rd!";

    public static async Task SeedAsync(
        AppDbContext context,
        IPasswordHasher passwordHasher,
        CancellationToken cancellationToken = default)
    {
        if (await context.Users.AnyAsync(cancellationToken))
            return;

        var demo = new User(DemoUsername, DemoEmail, passwordHasher.Hash(DemoPassword));
        context.Users.Add(demo);

        var today = DateTime.UtcNow;
        context.Tasks.AddRange(
            new TaskItem("Prepare quarterly report", "Compile Q3 numbers for the board", today.AddDays(3), demo.Id, TaskItemStatus.InProgress),
            new TaskItem("Book dentist appointment", null, today.AddDays(7), demo.Id, TaskItemStatus.Pending),
            new TaskItem("Renew gym membership", "Expires at month end", today.AddDays(14), demo.Id, TaskItemStatus.Pending),
            // A completed task whose deadline is in the past — legal because the
            // "not in the past" rule applies only to creation via the API.
            new TaskItem("Submit expense report", "August travel expenses", today.AddDays(-2), demo.Id, TaskItemStatus.Completed));

        await context.SaveChangesAsync(cancellationToken);
    }
}
