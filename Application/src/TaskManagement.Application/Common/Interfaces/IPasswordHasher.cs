namespace TaskManagement.Application.Common.Interfaces;

/// <summary>
/// Hashes and verifies passwords. Implemented in Infrastructure with
/// ASP.NET Core's PasswordHasher&lt;T&gt;. The Application layer never sees the
/// hashing algorithm.
/// </summary>
public interface IPasswordHasher
{
    string Hash(string password);

    bool Verify(string passwordHash, string providedPassword);
}
