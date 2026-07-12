namespace TaskManagement.Domain.Entities;

/// <summary>
/// An account that owns tasks. Credentials are never stored in clear text;
/// only the hash produced by the Infrastructure password hasher is kept.
/// </summary>
public class User
{
    public Guid Id { get; private set; }
    public string Username { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;
    public DateTime CreatedAt { get; private set; }

    // Parameterless constructor for EF Core materialization.
    private User()
    {
    }

    public User(string username, string email, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentException("Username is required.", nameof(username));
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email is required.", nameof(email));
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("Password hash is required.", nameof(passwordHash));

        Id = Guid.NewGuid();
        Username = username.Trim();
        Email = email.Trim();
        PasswordHash = passwordHash;
        CreatedAt = DateTime.UtcNow;
    }
}
