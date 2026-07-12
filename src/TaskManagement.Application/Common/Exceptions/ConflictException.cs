namespace TaskManagement.Application.Common.Exceptions;

/// <summary>
/// Thrown when a request conflicts with existing state, e.g. registering a
/// username or email that is already taken. Maps to 409 Conflict.
/// </summary>
public class ConflictException : Exception
{
    public ConflictException(string message) : base(message)
    {
    }
}
