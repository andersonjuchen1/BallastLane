using TaskManagement.Application.Common.Interfaces;

namespace TaskManagement.Infrastructure.Persistence;

/// <summary>
/// Commits staged changes on the shared (scoped) DbContext. Because the
/// repositories and this unit of work resolve the same DbContext per request,
/// their staged changes are committed together.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => _context.SaveChangesAsync(cancellationToken);
}
