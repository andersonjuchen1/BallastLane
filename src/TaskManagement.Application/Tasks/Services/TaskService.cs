using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Application.Tasks.Dtos;
using TaskManagement.Application.Tasks.Interfaces;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Enums;

namespace TaskManagement.Application.Tasks.Services;

public class TaskService : ITaskService
{
    private readonly ITaskRepository _tasks;
    private readonly ICurrentUserService _currentUser;

    public TaskService(ITaskRepository tasks, ICurrentUserService currentUser)
    {
        _tasks = tasks;
        _currentUser = currentUser;
    }

    public async Task<TaskResponse> CreateAsync(CreateTaskRequest request, CancellationToken cancellationToken = default)
    {
        // Ownership comes from the authenticated principal, never the request body.
        var task = new TaskItem(
            request.Title,
            request.Description,
            request.DueDate,
            _currentUser.UserId,
            request.Status);

        await _tasks.AddAsync(task, cancellationToken);

        return TaskResponse.FromEntity(task);
    }

    public Task<IReadOnlyList<TaskResponse>> GetAllAsync(TaskItemStatus? status, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task<TaskResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task<TaskResponse> UpdateAsync(Guid id, UpdateTaskRequest request, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();
}
