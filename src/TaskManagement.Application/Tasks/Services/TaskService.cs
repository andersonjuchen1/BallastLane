using TaskManagement.Application.Common.Exceptions;
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
    private readonly IUnitOfWork _unitOfWork;

    public TaskService(ITaskRepository tasks, ICurrentUserService currentUser, IUnitOfWork unitOfWork)
    {
        _tasks = tasks;
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
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

        _tasks.Add(task);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return TaskResponse.FromEntity(task);
    }

    public async Task<IReadOnlyList<TaskResponse>> GetAllAsync(TaskItemStatus? status, CancellationToken cancellationToken = default)
    {
        var tasks = await _tasks.GetByUserAsync(_currentUser.UserId, status, cancellationToken);

        return tasks.Select(TaskResponse.FromEntity).ToList();
    }

    public async Task<TaskResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var task = await GetOwnedTaskOrThrowAsync(id, cancellationToken);

        return TaskResponse.FromEntity(task);
    }

    public async Task<TaskResponse> UpdateAsync(Guid id, UpdateTaskRequest request, CancellationToken cancellationToken = default)
    {
        var task = await GetOwnedTaskOrThrowAsync(id, cancellationToken);

        task.Update(request.Title, request.Description, request.DueDate, request.Status);
        _tasks.Update(task);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return TaskResponse.FromEntity(task);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var task = await GetOwnedTaskOrThrowAsync(id, cancellationToken);

        _tasks.Remove(task);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Loads a task and enforces ownership. A task that is missing OR owned by
    /// another user is reported identically as NotFound, so existence is never
    /// leaked to a non-owner.
    /// </summary>
    private async Task<TaskItem> GetOwnedTaskOrThrowAsync(Guid id, CancellationToken cancellationToken)
    {
        var task = await _tasks.GetByIdAsync(id, cancellationToken);

        if (task is null || task.UserId != _currentUser.UserId)
            throw NotFoundException.ForEntity(nameof(TaskItem), id);

        return task;
    }
}
