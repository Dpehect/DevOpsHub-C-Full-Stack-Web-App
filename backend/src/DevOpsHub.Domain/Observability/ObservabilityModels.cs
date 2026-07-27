namespace DevOpsHub.Domain.Observability;

public enum LogLevelType { Trace, Debug, Information, Warning, Error, Critical }

public sealed class SystemLog : Entity
{
    public LogLevelType Level { get; set; }
    public string Message { get; set; } = string.Empty;
    public string Category { get; set; } = "Application";
    public string? Exception { get; set; }
    public string? RequestId { get; set; }
    public string? UserId { get; set; }
    public string? Path { get; set; }
    public string? Method { get; set; }
    public int? StatusCode { get; set; }
    public long? DurationMs { get; set; }
    public string? MetadataJson { get; set; }
}

public sealed class AuditEntry : Entity
{
    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string? EntityId { get; set; }
    public string? UserId { get; set; }
    public string? UserEmail { get; set; }
    public string? IpAddress { get; set; }
    public string? RequestId { get; set; }
    public string? ChangesJson { get; set; }
    public bool Succeeded { get; set; }
}
