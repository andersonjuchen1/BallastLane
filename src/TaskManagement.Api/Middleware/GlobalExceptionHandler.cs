using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TaskManagement.Application.Common.Exceptions;

namespace TaskManagement.Api.Middleware;

/// <summary>
/// Translates known application exceptions into ProblemDetails responses and
/// hides everything else behind a generic 500. Stack traces are logged, never
/// returned to the caller.
/// </summary>
public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (status, title) = exception switch
        {
            NotFoundException => (StatusCodes.Status404NotFound, exception.Message),
            ConflictException => (StatusCodes.Status409Conflict, exception.Message),
            InvalidCredentialsException => (StatusCodes.Status401Unauthorized, exception.Message),
            _ => (StatusCodes.Status500InternalServerError, "An unexpected error occurred."),
        };

        // Only unexpected (500) errors are logged at error level with detail.
        if (status == StatusCodes.Status500InternalServerError)
            _logger.LogError(exception, "Unhandled exception processing {Path}", httpContext.Request.Path);

        var problem = new ProblemDetails
        {
            Status = status,
            Title = title,
            Instance = httpContext.Request.Path,
        };

        httpContext.Response.StatusCode = status;
        await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);

        return true;
    }
}
