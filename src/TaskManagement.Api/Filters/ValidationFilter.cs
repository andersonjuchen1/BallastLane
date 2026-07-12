using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace TaskManagement.Api.Filters;

/// <summary>
/// Runs the FluentValidation validator (if one is registered) for each action
/// argument before the action executes. On failure it short-circuits with a
/// 400 and a field-level ValidationProblemDetails. This keeps validation at the
/// API boundary and out of the controllers and services.
/// </summary>
public class ValidationFilter : IAsyncActionFilter
{
    private readonly IServiceProvider _services;

    public ValidationFilter(IServiceProvider services)
    {
        _services = services;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        foreach (var argument in context.ActionArguments.Values)
        {
            if (argument is null)
                continue;

            var validatorType = typeof(IValidator<>).MakeGenericType(argument.GetType());

            if (_services.GetService(validatorType) is IValidator validator)
            {
                var validationContext = new ValidationContext<object>(argument);
                var result = await validator.ValidateAsync(validationContext, context.HttpContext.RequestAborted);

                if (!result.IsValid)
                {
                    var errors = result.Errors
                        .GroupBy(e => e.PropertyName)
                        .ToDictionary(
                            group => group.Key,
                            group => group.Select(e => e.ErrorMessage).ToArray());

                    context.Result = new BadRequestObjectResult(new ValidationProblemDetails(errors));
                    return;
                }
            }
        }

        await next();
    }
}
