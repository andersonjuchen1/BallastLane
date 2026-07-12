using Microsoft.AspNetCore.Identity;
using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Domain.Entities;

namespace TaskManagement.Infrastructure.Security;

/// <summary>
/// Wraps ASP.NET Core's PasswordHasher&lt;T&gt;, which applies a salted PBKDF2
/// hash. The generic user argument is unused by the default implementation, so
/// a placeholder is passed.
/// </summary>
public class PasswordHasher : IPasswordHasher
{
    private static readonly User Placeholder = null!;
    private readonly PasswordHasher<User> _hasher = new();

    public string Hash(string password)
        => _hasher.HashPassword(Placeholder, password);

    public bool Verify(string passwordHash, string providedPassword)
    {
        var result = _hasher.VerifyHashedPassword(Placeholder, passwordHash, providedPassword);
        return result != PasswordVerificationResult.Failed;
    }
}
