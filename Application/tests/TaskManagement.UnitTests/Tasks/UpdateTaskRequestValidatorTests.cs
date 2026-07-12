using FluentValidation.TestHelper;
using TaskManagement.Application.Tasks.Dtos;
using TaskManagement.Application.Tasks.Validators;
using TaskManagement.Domain.Enums;

namespace TaskManagement.UnitTests.Tasks;

public class UpdateTaskRequestValidatorTests
{
    private readonly UpdateTaskRequestValidator _validator = new();

    private static UpdateTaskRequest ValidRequest() =>
        new("Revise report", "updated", DateTime.UtcNow.AddDays(2), TaskItemStatus.InProgress);

    [Fact]
    public void Should_have_error_when_title_is_empty()
    {
        var request = ValidRequest() with { Title = "" };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public void Should_have_error_when_status_is_out_of_range()
    {
        var request = ValidRequest() with { Status = (TaskItemStatus)99 };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Status);
    }

    // Unlike creation, an update does NOT reject a past due date: an existing
    // task may legitimately be edited (e.g. marked Completed) after its deadline.
    [Fact]
    public void Should_not_reject_a_past_due_date_on_update()
    {
        var request = ValidRequest() with { DueDate = DateTime.UtcNow.AddDays(-10) };

        var result = _validator.TestValidate(request);

        result.ShouldNotHaveValidationErrorFor(x => x.DueDate);
    }

    [Fact]
    public void Should_not_have_errors_for_a_valid_request()
    {
        var result = _validator.TestValidate(ValidRequest());

        result.ShouldNotHaveAnyValidationErrors();
    }
}
