using FluentValidation;
using TaskManagement.Application.Tasks.Dtos;

namespace TaskManagement.Application.Tasks.Validators;

public class CreateTaskRequestValidator : AbstractValidator<CreateTaskRequest>
{
    public CreateTaskRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200);

        RuleFor(x => x.Description)
            .MaximumLength(2000);

        // Compared against "now" at validation time (lambda, not a captured value)
        // so a long-lived validator instance never uses a stale timestamp.
        RuleFor(x => x.DueDate)
            .GreaterThanOrEqualTo(_ => DateTime.UtcNow)
            .WithMessage("Due date cannot be in the past.");

        RuleFor(x => x.Status)
            .IsInEnum();
    }
}
