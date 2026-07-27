using DevOpsHub.Domain.Observability;
using Xunit;

namespace DevOpsHub.Tests;

public sealed class ObservabilityTests
{
    [Fact]
    public void Log_levels_include_operational_severities()
    {
        Assert.Contains(LogLevelType.Warning, Enum.GetValues<LogLevelType>());
        Assert.Contains(LogLevelType.Critical, Enum.GetValues<LogLevelType>());
    }

    [Fact]
    public void Audit_entry_starts_with_unique_identity()
    {
        var entry = new AuditEntry { Action = "PATCH", EntityType = "items", Succeeded = true };
        Assert.NotEqual(Guid.Empty, entry.Id);
        Assert.True(entry.Succeeded);
    }
}
