using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using TaskManagement.Application.Common.Interfaces;

namespace TaskManagement.Api.Services;

/// <summary>
/// Resolves the authenticated user's id from the JWT "sub" claim on the current
/// request. This is the single server-side source of ownership.
/// </summary>
public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid UserId
    {
        get
        {
            var principal = _httpContextAccessor.HttpContext?.User;

            // Inbound claim mapping is disabled, so "sub" stays "sub". Fall back
            // to NameIdentifier in case mapping is ever re-enabled.
            var value = principal?.FindFirstValue(JwtRegisteredClaimNames.Sub)
                        ?? principal?.FindFirstValue(ClaimTypes.NameIdentifier);

            return Guid.TryParse(value, out var id)
                ? id
                : throw new UnauthorizedAccessException("No authenticated user id was found on the request.");
        }
    }
}
