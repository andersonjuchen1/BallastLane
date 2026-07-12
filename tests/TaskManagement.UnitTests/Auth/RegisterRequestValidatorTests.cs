using FluentValidation.TestHelper;
using TaskManagement.Application.Auth.Dtos;
using TaskManagement.Application.Auth.Validators;

namespace TaskManagement.UnitTests.Auth;

public class RegisterRequestValidatorTests
{
    private readonly RegisterRequestValidator _validator = new();

    private static RegisterRequest ValidRequest() =>
        new("alice", "alice@example.com", "Passw0rd!");

    [Fact]
    public void Should_have_error_when_username_is_empty()
    {
        var result = _validator.TestValidate(ValidRequest() with { Username = "" });

        result.ShouldHaveValidationErrorFor(x => x.Username);
    }

    [Fact]
    public void Should_have_error_when_email_is_not_a_valid_address()
    {
        var result = _validator.TestValidate(ValidRequest() with { Email = "not-an-email" });

        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Should_have_error_when_password_is_too_short()
    {
        var result = _validator.TestValidate(ValidRequest() with { Password = "short" });

        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Should_not_have_errors_for_a_valid_request()
    {
        var result = _validator.TestValidate(ValidRequest());

        result.ShouldNotHaveAnyValidationErrors();
    }
}
