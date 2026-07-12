using TaskManagement.Domain.Enums;

namespace TaskManagement.Domain.Entities;

/// <summary>
/// A personal task owned by a single user. Named TaskItem (not Task) to avoid
/// colliding with System.Threading.Tasks.Task.
/// </summary>
public class TaskItem
{
    public Guid Id { get; private set; }
    public string Title { get; private set; } = null!;
    public string? Description { get; private set; }
    public TaskItemStatus Status { get; private set; }
    public DateTime DueDate { get; private set; }
    public Guid UserId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    // Parameterless constructor for EF Core materialization.
    private TaskItem()
    {
    }

    public TaskItem(
        string title,
        string? description,
        DateTime dueDate,
        Guid userId,
        TaskItemStatus status = TaskItemStatus.Pending)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title is required.", nameof(title));
        if (userId == Guid.Empty)
            throw new ArgumentException("A task must belong to a user.", nameof(userId));

        Id = Guid.NewGuid();
        Title = title.Trim();
        Description = description;
        DueDate = dueDate;
        UserId = userId;
        Status = status;
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Applies an edit to the mutable fields and stamps the update time.
    /// Ownership (UserId) and identity (Id) are immutable after creation.
    /// </summary>
    public void Update(string title, string? description, DateTime dueDate, TaskItemStatus status)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title is required.", nameof(title));

        Title = title.Trim();
        Description = description;
        DueDate = dueDate;
        Status = status;
        UpdatedAt = DateTime.UtcNow;
    }
}
