using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;
using DevOpsHub.Application.Auth;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DevOpsHub.Api.Controllers;

[ApiController]
[EnableRateLimiting("auth")]
[Route("api/auth")]
public sealed class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request, IValidator<RegisterRequest> validator, CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid) return ValidationProblem(validation.ToDictionary());
        var result = await authService.RegisterAsync(request, cancellationToken);
        return result.Succeeded ? Ok(result.Value) : Conflict(result.Error);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request, IValidator<LoginRequest> validator, CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid) return ValidationProblem(validation.ToDictionary());
        var result = await authService.LoginAsync(request, cancellationToken);
        return result.Succeeded ? Ok(result.Value) : Unauthorized(result.Error);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(RefreshRequest request, CancellationToken cancellationToken)
    {
        var result = await authService.RefreshAsync(request, cancellationToken);
        return result.Succeeded ? Ok(result.Value) : Unauthorized(result.Error);
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout(LogoutRequest request, CancellationToken cancellationToken)
    {
        await authService.LogoutAsync(request, cancellationToken);
        return NoContent();
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> Me(CancellationToken cancellationToken)
    {
        var sub = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
        if (!Guid.TryParse(sub, out var userId)) return Unauthorized();
        var user = await authService.GetCurrentUserAsync(userId, cancellationToken);
        return user is null ? Unauthorized() : Ok(user);
    }
}
