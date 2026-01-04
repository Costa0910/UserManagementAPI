using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace UserManagementAPI.Middleware;

public class RequestResponseLoggingMiddleware(RequestDelegate next, ILogger<RequestResponseLoggingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var sw = Stopwatch.StartNew();
        var request = context.Request;
        logger.LogInformation("Request {Method} {Path}", request.Method, request.Path);

        await next(context);

        sw.Stop();
        logger.LogInformation("Response {StatusCode} for {Method} {Path} in {Elapsed} ms", context.Response.StatusCode, request.Method, request.Path, sw.ElapsedMilliseconds);
    }
}

