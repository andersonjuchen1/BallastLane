using FluentValidation;
using TaskManagement.Application.Tasks.Dtos;

namespace TaskManagement.Application.Tasks.Validators;

public class UpdateTaskRequestValidator : AbstractValidator<UpdateTaskRequest>
{
    public UpdateTaskRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200);

        RuleFor(x => x.Description)
            .MaximumLength(2000);

        RuleFor(x => x.Status)
            .IsInEnum();

        // Note: no "due date in the past" rule here. That constraint applies to
        // creation only; an existing task may be edited after its deadline.
    }
}
