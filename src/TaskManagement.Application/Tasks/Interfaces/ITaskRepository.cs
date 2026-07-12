using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Enums;

namespace TaskManagement.Application.Tasks.Interfaces;

/// <summary>
/// Persistence boundary for tasks. Implemented in Infrastructure with EF Core.
/// Reads are async; writes stage changes only — the caller commits them through
/// <see cref="IUnitOfWork"/>.
/// </summary>
public interface ITaskRepository
{
    Task<TaskItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TaskItem>> GetByUserAsync(
        Guid userId,
        TaskItemStatus? status,
        CancellationToken cancellationToken = default);

    void Add(TaskItem task);

    void Update(TaskItem task);

    void Remove(TaskItem task);
}
