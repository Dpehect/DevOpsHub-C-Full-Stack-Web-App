using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DevOpsHub.Infrastructure.Persistence;

public static class DatabaseRetryExtensions
{
    public static async Task WaitForDatabaseAsync(
        this IServiceProvider services,
        int maxAttempts = 10,
        CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var logger = scope.ServiceProvider
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("DatabaseStartup");

        var delay = TimeSpan.FromSeconds(2);

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                if (await db.Database.CanConnectAsync(cancellationToken))
                    return;
            }
            catch (Exception ex) when (attempt < maxAttempts)
            {
                logger.LogWarning(
                    ex,
                    "Database connection attempt {Attempt}/{MaxAttempts} failed.",
                    attempt,
                    maxAttempts);
            }

            if (attempt == maxAttempts)
                throw new InvalidOperationException(
                    "Database is unavailable after retry attempts.");

            await Task.Delay(delay, cancellationToken);
            delay = TimeSpan.FromSeconds(Math.Min(delay.TotalSeconds * 2, 30));
        }
    }
}
