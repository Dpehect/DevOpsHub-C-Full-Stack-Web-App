namespace DevOpsHub.Domain;

public abstract class Entity
{
    public Guid Id { get; protected set; } = Guid.NewGuid();
    public DateTime CreatedAtUtc { get; protected set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; protected set; }

    protected void Touch() => UpdatedAtUtc = DateTime.UtcNow;
}
