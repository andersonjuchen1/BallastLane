using TaskManagement.Domain.Entities;

namespace TaskManagement.Application.Auth.Dtos;

/// <summary>
/// Public view of a user. Never includes the password hash.
/// </summary>
public record UserResponse(Guid Id, string Username, string Email, DateTime CreatedAt)
{
    public static UserResponse FromEntity(User user) =>
        new(user.Id, user.Username, user.Email, user.CreatedAt);
}
