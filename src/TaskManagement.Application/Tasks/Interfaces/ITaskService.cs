using TaskManagement.Application.Tasks.Dtos;
using TaskManagement.Domain.Enums;

namespace TaskManagement.Application.Tasks.Interfaces;

/// <summary>
/// Task use cases. Every operation is implicitly scoped to the authenticated
/// user; tasks belonging to other users are treated as if they do not exist.
/// </summary>
public interface ITaskService
{
    Task<TaskResponse> CreateAsync(CreateTaskRequest request, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TaskResponse>> GetAllAsync(TaskItemStatus? status, CancellationToken cancellationToken = default);

    Task<TaskResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<TaskResponse> UpdateAsync(Guid id, UpdateTaskRequest request, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
