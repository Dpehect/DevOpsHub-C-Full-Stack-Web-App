namespace DevOpsHub.Application.Auth;

public sealed record RegisterRequest(string Email, string DisplayName, string Password);
public sealed record LoginRequest(string Email, string Password);
public sealed record RefreshRequest(string RefreshToken);
public sealed record LogoutRequest(string RefreshToken);
public sealed record UserResponse(Guid Id, string Email, string DisplayName, string Role);
public sealed record AuthResponse(string AccessToken, string RefreshToken, DateTime AccessTokenExpiresAtUtc, UserResponse User);
