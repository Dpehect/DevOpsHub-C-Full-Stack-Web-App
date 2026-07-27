using DevOpsHub.Api.Middleware;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace DevOpsHub.Tests;

public sealed class SecurityHeadersMiddlewareTests
{
    [Fact]
    public async Task Adds_defensive_headers()
    {
        var context = new DefaultHttpContext();
        var middleware = new SecurityHeadersMiddleware(_ => Task.CompletedTask);

        await middleware.InvokeAsync(context);

        Assert.Equal("nosniff", context.Response.Headers["X-Content-Type-Options"]);
        Assert.Equal("DENY", context.Response.Headers["X-Frame-Options"]);
        Assert.Contains("frame-ancestors 'none'", context.Response.Headers["Content-Security-Policy"].ToString());
    }
}
