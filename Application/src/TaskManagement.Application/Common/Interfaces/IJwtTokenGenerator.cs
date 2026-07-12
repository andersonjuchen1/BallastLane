using TaskManagement.Application.Common.Security;
using TaskManagement.Domain.Entities;

namespace TaskManagement.Application.Common.Interfaces;

/// <summary>
/// Issues a signed JWT for a user. Implemented in Infrastructure.
/// </summary>
public interface IJwtTokenGenerator
{
    TokenResult Generate(User user);
}
