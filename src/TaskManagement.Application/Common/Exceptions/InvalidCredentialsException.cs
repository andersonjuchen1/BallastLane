namespace TaskManagement.Application.Common.Exceptions;

/// <summary>
/// Thrown when login fails, whether the username is unknown or the password is
/// wrong. The message is deliberately generic and identical in both cases so a
/// caller cannot tell which field was incorrect. Maps to 401 Unauthorized.
/// </summary>
public class InvalidCredentialsException : Exception
{
    public InvalidCredentialsException() : base("Invalid username or password.")
    {
    }
}
