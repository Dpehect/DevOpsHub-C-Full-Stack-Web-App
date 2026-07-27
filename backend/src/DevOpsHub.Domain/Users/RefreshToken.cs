using DevOpsHub.Domain;

namespace DevOpsHub.Domain.Users;

public sealed class RefreshToken : Entity
{
    private RefreshToken() { }

    public RefreshToken(Guid userId, string tokenHash, DateTime expiresAtUtc)
    {
        UserId = userId;
        TokenHash = tokenHash;
        ExpiresAtUtc = expiresAtUtc;
    }

    public Guid UserId { get; private set; }
    public AppUser User { get; private set; } = null!;
    public string TokenHash { get; private set; } = string.Empty;
    public DateTime ExpiresAtUtc { get; private set; }
    public DateTime? RevokedAtUtc { get; private set; }
    public string? ReplacedByTokenHash { get; private set; }

    public bool IsActive => RevokedAtUtc is null && DateTime.UtcNow < ExpiresAtUtc;

    public void Revoke(string? replacedByTokenHash = null)
    {
        RevokedAtUtc = DateTime.UtcNow;
        ReplacedByTokenHash = replacedByTokenHash;
        Touch();
    }
}
