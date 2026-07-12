using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using TaskManagement.Application.Auth.Interfaces;
using TaskManagement.Application.Auth.Services;
using TaskManagement.Application.Tasks.Interfaces;
using TaskManagement.Application.Tasks.Services;

namespace TaskManagement.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ITaskService, TaskService>();

        // Registers every AbstractValidator in this assembly.
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        return services;
    }
}
