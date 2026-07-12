namespace TaskManagement.Application.Common.Interfaces;

/// <summary>
/// Exposes the authenticated user's id, resolved from JWT claims by the Api layer.
/// Ownership is always taken from here — never from a request body — so a caller
/// can never act on behalf of another user.
/// </summary>
public interface ICurrentUserService
{
    Guid UserId { get; }
}
