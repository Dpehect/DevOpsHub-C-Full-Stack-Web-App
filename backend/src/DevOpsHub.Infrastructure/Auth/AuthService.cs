using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using DevOpsHub.Application.Auth;
using DevOpsHub.Application.Common;
using DevOpsHub.Domain.Users;
using DevOpsHub.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace DevOpsHub.Infrastructure.Auth;

public sealed class AuthService(
    AppDbContext db,
    IPasswordHasher<AppUser> passwordHasher,
    IOptions<JwtOptions> options) : IAuthService
{
    private readonly JwtOptions _options = options.Value;

    public async Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken)
    {
        var email = AppUser.NormalizeEmail(request.Email);
        if (await db.Users.AnyAsync(x => x.Email == email, cancellationToken))
            return Result<AuthResponse>.Failure("auth.email_exists", "An account with this email already exists.");

        var user = new AppUser(email, request.DisplayName, string.Empty);
        var hash = passwordHasher.HashPassword(user, request.Password);
        typeof(AppUser).GetProperty(nameof(AppUser.PasswordHash))!.SetValue(user, hash);

        db.Users.Add(user);
        var response = await CreateSessionAsync(user, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return Result<AuthResponse>.Success(response);
    }

    public async Task<Result<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var email = AppUser.NormalizeEmail(request.Email);
        var user = await db.Users.Include(x => x.RefreshTokens).SingleOrDefaultAsync(x => x.Email == email, cancellationToken);
        if (user is null || !user.IsActive)
            return Result<AuthResponse>.Failure("auth.invalid_credentials", "Email or password is incorrect.");

        var result = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.Password);
        if (result == PasswordVerificationResult.Failed)
            return Result<AuthResponse>.Failure("auth.invalid_credentials", "Email or password is incorrect.");

        user.RecordLogin();
        var response = await CreateSessionAsync(user, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
        return Result<AuthResponse>.Success(response);
    }

    public async Task<Result<AuthResponse>> RefreshAsync(RefreshRequest request, CancellationToken cancellationToken)
    {
        var incomingHash = HashToken(request.RefreshToken);
        var stored = await db.RefreshTokens.Include(x => x.User)
            .SingleOrDefaultAsync(x => x.TokenHash == incomingHash, cancellationToken);

        if (stored is null || !stored.IsActive || !stored.User.IsActive)
            return Result<AuthResponse>.Failure("auth.invalid_refresh_token", "Refresh token is invalid or expired.");

        var rawReplacement = GenerateRefreshToken();
        var replacementHash = HashToken(rawReplacement);
        stored.Revoke(replacementHash);
        var replacement = new RefreshToken(stored.UserId, replacementHash, DateTime.UtcNow.AddDays(_options.RefreshTokenDays));
        db.RefreshTokens.Add(replacement);

        var response = BuildAuthResponse(stored.User, rawReplacement);
        await db.SaveChangesAsync(cancellationToken);
        return Result<AuthResponse>.Success(response);
    }

    public async Task<Result> LogoutAsync(LogoutRequest request, CancellationToken cancellationToken)
    {
        var hash = HashToken(request.RefreshToken);
        var token = await db.RefreshTokens.SingleOrDefaultAsync(x => x.TokenHash == hash, cancellationToken);
        if (token is not null && token.IsActive) token.Revoke();
        await db.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<UserResponse?> GetCurrentUserAsync(Guid userId, CancellationToken cancellationToken) =>
        await db.Users.Where(x => x.Id == userId && x.IsActive)
            .Select(x => new UserResponse(x.Id, x.Email, x.DisplayName, x.Role))
            .SingleOrDefaultAsync(cancellationToken);

    private Task<AuthResponse> CreateSessionAsync(AppUser user, CancellationToken cancellationToken)
    {
        var raw = GenerateRefreshToken();
        db.RefreshTokens.Add(new RefreshToken(user.Id, HashToken(raw), DateTime.UtcNow.AddDays(_options.RefreshTokenDays)));
        return Task.FromResult(BuildAuthResponse(user, raw));
    }

    private AuthResponse BuildAuthResponse(AppUser user, string refreshToken)
    {
        var expires = DateTime.UtcNow.AddMinutes(_options.AccessTokenMinutes);
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Name, user.DisplayName),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SigningKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(_options.Issuer, _options.Audience, claims, expires: expires, signingCredentials: credentials);
        var accessToken = new JwtSecurityTokenHandler().WriteToken(token);
        return new AuthResponse(accessToken, refreshToken, expires, new UserResponse(user.Id, user.Email, user.DisplayName, user.Role));
    }

    private static string GenerateRefreshToken() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
    private static string HashToken(string token) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));
}
