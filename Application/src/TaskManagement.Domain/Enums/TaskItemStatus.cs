namespace TaskManagement.Domain.Enums;

/// <summary>
/// Lifecycle state of a task. Named TaskItemStatus (not TaskStatus) to avoid
/// colliding with System.Threading.Tasks.TaskStatus.
/// </summary>
public enum TaskItemStatus
{
    Pending = 0,
    InProgress = 1,
    Completed = 2
}
