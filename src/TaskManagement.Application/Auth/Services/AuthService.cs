using TaskManagement.Application.Auth.Dtos;
using TaskManagement.Application.Auth.Interfaces;
using TaskManagement.Application.Common.Exceptions;
using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Application.Users.Interfaces;
using TaskManagement.Domain.Entities;

namespace TaskManagement.Application.Auth.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _users;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _tokenGenerator;
    private readonly ICurrentUserService _currentUser;

    public AuthService(
        IUserRepository users,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator tokenGenerator,
        ICurrentUserService currentUser)
    {
        _users = users;
        _passwordHasher = passwordHasher;
        _tokenGenerator = tokenGenerator;
        _currentUser = currentUser;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        if (await _users.ExistsByUsernameAsync(request.Username, cancellationToken))
            throw new ConflictException("Username is already taken.");

        if (await _users.ExistsByEmailAsync(request.Email, cancellationToken))
            throw new ConflictException("Email is already registered.");

        var passwordHash = _passwordHasher.Hash(request.Password);
        var user = new User(request.Username, request.Email, passwordHash);
        await _users.AddAsync(user, cancellationToken);

        return IssueToken(user);
    }

    private AuthResponse IssueToken(User user)
    {
        var token = _tokenGenerator.Generate(user);
        return new AuthResponse(token.Token, token.ExpiresAtUtc);
    }

    public Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task<UserResponse> GetCurrentUserAsync(CancellationToken cancellationToken = default)
        => throw new NotImplementedException();
}
