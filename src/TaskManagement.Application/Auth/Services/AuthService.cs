using TaskManagement.Application.Auth.Dtos;
using TaskManagement.Application.Auth.Interfaces;
using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Application.Users.Interfaces;

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

    public Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
        => throw new NotImplementedException();

    public Task<UserResponse> GetCurrentUserAsync(CancellationToken cancellationToken = default)
        => throw new NotImplementedException();
}
