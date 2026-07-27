namespace DevOpsHub.Api.Middleware;

public sealed class SecurityHeadersMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var headers = context.Response.Headers;
        headers["X-Content-Type-Options"] = "nosniff";
        headers["X-Frame-Options"] = "DENY";
        headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
        headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";
        headers["Content-Security-Policy"] = "default-src 'none'; frame-ancestors 'none'; base-uri 'none'; form-action 'self'";
        headers["Cross-Origin-Opener-Policy"] = "same-origin";
        headers["Cross-Origin-Resource-Policy"] = "same-site";
        context.Response.Headers.Remove("Server");
        await next(context);
    }
}
