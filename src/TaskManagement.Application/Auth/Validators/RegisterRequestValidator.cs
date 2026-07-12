using FluentValidation;
using TaskManagement.Application.Auth.Dtos;

namespace TaskManagement.Application.Auth.Validators;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        // Rules added in the green step.
    }
}
