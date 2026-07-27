using Microsoft.Extensions.DependencyInjection;

namespace DevOpsHub.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        return services;
    }
}
