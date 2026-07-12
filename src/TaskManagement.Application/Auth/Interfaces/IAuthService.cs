using TaskManagement.Application.Auth.Dtos;

namespace TaskManagement.Application.Auth.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);

    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);

    Task<UserResponse> GetCurrentUserAsync(CancellationToken cancellationToken = default);
}
