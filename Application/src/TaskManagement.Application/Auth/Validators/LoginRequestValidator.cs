using FluentValidation;
using TaskManagement.Application.Auth.Dtos;

namespace TaskManagement.Application.Auth.Validators;

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        // Presence only. Whether the credentials are actually correct is an
        // authentication concern (401), not input validation (400).
        RuleFor(x => x.Username).NotEmpty();
        RuleFor(x => x.Password).NotEmpty();
    }
}
