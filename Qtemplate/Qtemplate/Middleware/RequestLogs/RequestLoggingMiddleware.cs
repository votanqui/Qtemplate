using System.Diagnostics;
using System.Security.Claims;
using Qtemplate.Domain.Entities;

namespace Qtemplate.Middleware.RequestLogs;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;
    private readonly RequestLogQueue _queue;

    private static readonly HashSet<string> _excludedPaths = new(StringComparer.OrdinalIgnoreCase)
    {
        "/health", "/favicon.ico", "/swagger", "/swagger/index.html", "/hubs"
    };

    public RequestLoggingMiddleware(
        RequestDelegate next,
        ILogger<RequestLoggingMiddleware> logger,
        RequestLogQueue queue)
    {
        _next = next;
        _logger = logger;
        _queue = queue;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value ?? string.Empty;

        if (_excludedPaths.Any(p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
        {
            await _next(context);
            return;
        }

        var sw = Stopwatch.StartNew();
        try
        {
            await _next(context);
        }
        finally
        {
            sw.Stop();
            try
            {
                var ip = context.Request.Headers["X-Forwarded-For"].FirstOrDefault()
                         ?? context.Connection.RemoteIpAddress?.ToString()
                         ?? "unknown";

                _queue.Enqueue(new RequestLog
                {
                    IpAddress = ip,
                    UserId = context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value,
                    Endpoint = path,
                    Method = context.Request.Method,
                    StatusCode = context.Response.StatusCode,
                    ResponseTimeMs = sw.ElapsedMilliseconds,
                    UserAgent = context.Request.Headers.UserAgent.ToString(),
                    Referer = context.Request.Headers.Referer.ToString(),
                    CreatedAt = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi enqueue RequestLog");
            }
        }
    }
}

public static class RequestLoggingMiddlewareExtensions
{
    public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder app)
        => app.UseMiddleware<RequestLoggingMiddleware>();
}