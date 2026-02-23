using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using ScholarFlow.Application.Common.Behaviours;
namespace ScholarFlow.Application;

/// <summary>
/// Dependency Injection configuration for Application layer
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Register MediatR
         services.AddMediatR(cfg => {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            // Add behaviors for logging, validation, etc.
            cfg.AddOpenBehavior(typeof(UnhandledExceptionBehaviour<,>)); // For exception logging
            cfg.AddOpenBehavior(typeof(ValidationBehaviour<,>));       // For FluentValidation integration
            cfg.AddOpenBehavior(typeof(PerformanceBehaviour<,>));      // For logging request performance
        });

        // Register FluentValidation validators
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        // Register AutoMapper (if needed later)
        // services.AddAutoMapper(Assembly.GetExecutingAssembly());

        return services;
    }
}

