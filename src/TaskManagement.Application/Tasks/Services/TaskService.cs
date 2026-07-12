using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Application.Tasks.Dtos;
using TaskManagement.Application.Tasks.Interfaces;
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

    public Task<TaskResponse> CreateAsync(CreateTaskRequest request, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task<IReadOnlyList<TaskResponse>> GetAllAsync(TaskItemStatus? status, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task<TaskResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task<TaskResponse> UpdateAsync(Guid id, UpdateTaskRequest request, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();
}
