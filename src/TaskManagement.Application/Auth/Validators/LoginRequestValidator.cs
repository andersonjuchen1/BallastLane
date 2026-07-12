using FluentValidation;
using TaskManagement.Application.Auth.Dtos;

namespace TaskManagement.Application.Auth.Validators;

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        // Rules added in the green step.
    }
}
