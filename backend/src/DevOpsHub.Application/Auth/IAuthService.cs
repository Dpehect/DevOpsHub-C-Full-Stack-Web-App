using DevOpsHub.Application.Common;

namespace DevOpsHub.Application.Auth;

public interface IAuthService
{
    Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken);
    Task<Result<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken);
    Task<Result<AuthResponse>> RefreshAsync(RefreshRequest request, CancellationToken cancellationToken);
    Task<Result> LogoutAsync(LogoutRequest request, CancellationToken cancellationToken);
    Task<UserResponse?> GetCurrentUserAsync(Guid userId, CancellationToken cancellationToken);
}
