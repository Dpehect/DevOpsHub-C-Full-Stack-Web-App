using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace DevOpsHub.Application.Common.Validation;

public static class ValidationExtensions
{
    public static IServiceCollection AddRequestValidation(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<ValidationExtensions>();
        return services;
    }
}
