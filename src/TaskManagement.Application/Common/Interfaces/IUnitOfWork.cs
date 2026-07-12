namespace TaskManagement.Application.Common.Interfaces;

/// <summary>
/// Commits all staged changes as a single transaction. Repositories stage work
/// (Add/Update/Remove); the service decides the transactional boundary by
/// calling SaveChangesAsync. Implemented in Infrastructure over the DbContext.
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
