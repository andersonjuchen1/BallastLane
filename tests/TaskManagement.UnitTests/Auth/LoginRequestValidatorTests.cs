using FluentValidation.TestHelper;
using TaskManagement.Application.Auth.Dtos;
using TaskManagement.Application.Auth.Validators;

namespace TaskManagement.UnitTests.Auth;

public class LoginRequestValidatorTests
{
    private readonly LoginRequestValidator _validator = new();

    [Fact]
    public void Should_have_error_when_username_is_empty()
    {
        var result = _validator.TestValidate(new LoginRequest("", "Passw0rd!"));

        result.ShouldHaveValidationErrorFor(x => x.Username);
    }

    [Fact]
    public void Should_have_error_when_password_is_empty()
    {
        var result = _validator.TestValidate(new LoginRequest("alice", ""));

        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Should_not_have_errors_for_a_valid_request()
    {
        var result = _validator.TestValidate(new LoginRequest("alice", "Passw0rd!"));

        result.ShouldNotHaveAnyValidationErrors();
    }
}
