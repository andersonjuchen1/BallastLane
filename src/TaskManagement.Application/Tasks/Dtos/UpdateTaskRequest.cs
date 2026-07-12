using TaskManagement.Domain.Enums;

namespace TaskManagement.Application.Tasks.Dtos;

/// <summary>
/// Input for updating a task. Like CreateTaskRequest it carries no user id;
/// the target task is located by route id and re-checked against the owner.
/// </summary>
public record UpdateTaskRequest(
    string Title,
    string? Description,
    DateTime DueDate,
    TaskItemStatus Status);
