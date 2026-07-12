using TaskManagement.Domain.Enums;

namespace TaskManagement.Application.Tasks.Dtos;

/// <summary>
/// Input for creating a task. Deliberately has no user id — ownership is taken
/// from the authenticated principal, not the request body.
/// </summary>
public record CreateTaskRequest(
    string Title,
    string? Description,
    DateTime DueDate,
    TaskItemStatus Status = TaskItemStatus.Pending);
