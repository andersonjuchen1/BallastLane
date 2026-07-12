using Microsoft.EntityFrameworkCore;
using TaskManagement.Application.Tasks.Interfaces;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Enums;

namespace TaskManagement.Infrastructure.Persistence.Repositories;

public class TaskRepository : ITaskRepository
{
    private readonly AppDbContext _context;

    public TaskRepository(AppDbContext context)
    {
        _context = context;
    }

    // Tracked: the caller may mutate and persist the returned entity.
    public async Task<TaskItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.Tasks.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

    // Read-only list: no change tracking, scoped to the owner, filtered by status.
    public async Task<IReadOnlyList<TaskItem>> GetByUserAsync(
        Guid userId,
        TaskItemStatus? status,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Tasks
            .AsNoTracking()
            .Where(t => t.UserId == userId);

        if (status.HasValue)
            query = query.Where(t => t.Status == status.Value);

        return await query
            .OrderBy(t => t.DueDate)
            .ToListAsync(cancellationToken);
    }

    public void Add(TaskItem task) => _context.Tasks.Add(task);

    public void Update(TaskItem task) => _context.Tasks.Update(task);

    public void Remove(TaskItem task) => _context.Tasks.Remove(task);
}
