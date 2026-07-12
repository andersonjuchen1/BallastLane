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
    private readonly IUnitOfWork _unitOfWork;

    public AuthService(
        IUserRepository users,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator tokenGenerator,
        ICurrentUserService currentUser,
        IUnitOfWork unitOfWork)
    {
        _users = users;
        _passwordHasher = passwordHasher;
        _tokenGenerator = tokenGenerator;
        _currentUser = currentUser;
        _unitOfWork = unitOfWork;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        if (await _users.ExistsByUsernameAsync(request.Username, cancellationToken))
            throw new ConflictException("Username is already taken.");

        if (await _users.ExistsByEmailAsync(request.Email, cancellationToken))
            throw new ConflictException("Email is already registered.");

        var passwordHash = _passwordHasher.Hash(request.Password);
        var user = new User(request.Username, request.Email, passwordHash);
        _users.Add(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return IssueToken(user);
    }

    private AuthResponse IssueToken(User user)
    {
        var token = _tokenGenerator.Generate(user);
        return new AuthResponse(token.Token, token.ExpiresAtUtc);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _users.GetByUsernameAsync(request.Username, cancellationToken);

        // Same exception for "no such user" and "wrong password" so the caller
        // cannot distinguish the two.
        if (user is null || !_passwordHasher.Verify(user.PasswordHash, request.Password))
            throw new InvalidCredentialsException();

        return IssueToken(user);
    }

    public async Task<UserResponse> GetCurrentUserAsync(CancellationToken cancellationToken = default)
    {
        var user = await _users.GetByIdAsync(_currentUser.UserId, cancellationToken);

        if (user is null)
            throw NotFoundException.ForEntity(nameof(User), _currentUser.UserId);

        return UserResponse.FromEntity(user);
    }
}
