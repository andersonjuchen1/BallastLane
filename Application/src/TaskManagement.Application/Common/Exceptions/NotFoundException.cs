namespace TaskManagement.Application.Common.Exceptions;

/// <summary>
/// Thrown when a requested entity does not exist, or exists but is not owned by
/// the current user. Both cases map to 404 so ownership is never revealed.
/// </summary>
public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message)
    {
    }

    public static NotFoundException ForEntity(string name, object key)
        => new($"{name} with id '{key}' was not found.");
}
