using FluentValidation.TestHelper;
using TaskManagement.Application.Tasks.Dtos;
using TaskManagement.Application.Tasks.Validators;
using TaskManagement.Domain.Enums;

namespace TaskManagement.UnitTests.Tasks;

public class CreateTaskRequestValidatorTests
{
    private readonly CreateTaskRequestValidator _validator = new();

    private static CreateTaskRequest ValidRequest() =>
        new("Write report", "Q3 summary", DateTime.UtcNow.AddDays(3), TaskItemStatus.Pending);

    [Fact]
    public void Should_have_error_when_title_is_empty()
    {
        var request = ValidRequest() with { Title = "" };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public void Should_have_error_when_due_date_is_in_the_past()
    {
        var request = ValidRequest() with { DueDate = DateTime.UtcNow.AddDays(-1) };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.DueDate);
    }

    [Fact]
    public void Should_not_have_errors_for_a_valid_request()
    {
        var result = _validator.TestValidate(ValidRequest());

        result.ShouldNotHaveAnyValidationErrors();
    }
}
