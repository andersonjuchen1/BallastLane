using FluentValidation;
using TaskManagement.Application.Tasks.Dtos;

namespace TaskManagement.Application.Tasks.Validators;

public class UpdateTaskRequestValidator : AbstractValidator<UpdateTaskRequest>
{
    public UpdateTaskRequestValidator()
    {
        // Rules added in the green step.
    }
}
