using FluentValidation;
using TaskManagement.Application.Tasks.Dtos;

namespace TaskManagement.Application.Tasks.Validators;

public class CreateTaskRequestValidator : AbstractValidator<CreateTaskRequest>
{
    public CreateTaskRequestValidator()
    {
        // Rules added in the green step.
    }
}
