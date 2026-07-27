using DevOpsHub.Domain;

namespace DevOpsHub.Domain.Users;

public sealed class AppUser : Entity
{
    private AppUser() { }

    public AppUser(string email, string displayName, string passwordHash, string role = Roles.Member)
    {
        Email = NormalizeEmail(email);
        DisplayName = displayName.Trim();
        PasswordHash = passwordHash;
        Role = role;
    }

    public string Email { get; private set; } = string.Empty;
    public string DisplayName { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public string Role { get; private set; } = Roles.Member;
    public bool IsActive { get; private set; } = true;
    public DateTime? LastLoginAtUtc { get; private set; }
    public ICollection<RefreshToken> RefreshTokens { get; private set; } = new List<RefreshToken>();

    public void RecordLogin() { LastLoginAtUtc = DateTime.UtcNow; Touch(); }
    public void ChangePassword(string hash) { PasswordHash = hash; Touch(); }
    public void Deactivate() { IsActive = false; Touch(); }
    public void Activate() { IsActive = true; Touch(); }
    public void ChangeRole(string role) { Role = role; Touch(); }

    public static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();
}

public static class Roles
{
    public const string Admin = "Admin";
    public const string Owner = "Owner";
    public const string Member = "Member";
}
