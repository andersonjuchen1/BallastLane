namespace TaskManagement.Application.Auth.Dtos;

/// <summary>
/// Returned on successful register/login: a bearer token and its expiry (UTC).
/// </summary>
public record AuthResponse(string AccessToken, DateTime ExpiresAtUtc);
