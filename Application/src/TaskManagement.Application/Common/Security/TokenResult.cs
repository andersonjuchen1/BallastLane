namespace TaskManagement.Application.Common.Security;

/// <summary>
/// The output of token generation: the encoded JWT and its UTC expiry.
/// Kept separate from AuthResponse so the generator has no dependency on DTOs.
/// </summary>
public record TokenResult(string Token, DateTime ExpiresAtUtc);
