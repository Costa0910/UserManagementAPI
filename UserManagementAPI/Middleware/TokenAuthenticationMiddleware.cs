using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using UserManagementAPI.Services;

namespace UserManagementAPI.Middleware;

public partial class TokenAuthenticationMiddleware(
    RequestDelegate next,
    ILogger<TokenAuthenticationMiddleware> logger,
    TokenStore tokenStore)
{
    private const string AuthorizationHeader = "Authorization";
    private const string BearerPrefix = "Bearer ";

    public async Task InvokeAsync(HttpContext context)
    {
        // Skip auth middleware for the /auth/token endpoint
        if (context.Request.Path.StartsWithSegments("/auth/token") && context.Request.Method == "POST")
        {
            await next(context);
            return;
        }

        // Validate bearer token for all other endpoints
        var token = ExtractToken(context);
        
        if (string.IsNullOrWhiteSpace(token))
        {
            LogMissingTokenForMethodPath(logger, context.Request.Method, context.Request.Path);
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Unauthorized: Missing or invalid token");
            return;
        }

        if (!tokenStore.Contains(token))
        {
            LogInvalidTokenForMethodPath(logger, context.Request.Method, context.Request.Path);
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Unauthorized: Invalid token");
            return;
        }

        // Token is valid, allow request to proceed
        await next(context);
    }

    private static string? ExtractToken(HttpContext context)
    {
        if (!context.Request.Headers.TryGetValue(AuthorizationHeader, out var authHeader) || authHeader.Count == 0)
        {
            return null;
        }

        var headerValue = authHeader.First();
        if (string.IsNullOrWhiteSpace(headerValue) || !headerValue.StartsWith(BearerPrefix, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return headerValue[BearerPrefix.Length..];
    }

    [LoggerMessage(LogLevel.Warning, "Missing or invalid token format for {method} {path}")]
    static partial void LogMissingTokenForMethodPath(ILogger<TokenAuthenticationMiddleware> logger, string method, PathString path);

    [LoggerMessage(LogLevel.Warning, "Invalid token for {method} {path}")]
    static partial void LogInvalidTokenForMethodPath(ILogger<TokenAuthenticationMiddleware> logger, string method, PathString path);
}
