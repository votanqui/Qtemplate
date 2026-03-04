using System.Diagnostics;
using System.Security.Claims;
using Qtemplate.Domain.Entities;
using Qtemplate.Infrastructure.Data;

namespace Qtemplate.Middleware;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    // Các endpoint không cần log (tránh spam DB)
    private static readonly HashSet<string> _excludedPaths = new(StringComparer.OrdinalIgnoreCase)
    {
        "/health", "/favicon.ico", "/swagger", "/swagger/index.html"
    };

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, AppDbContext dbContext)
    {
        var path = context.Request.Path.Value ?? string.Empty;

        // Bỏ qua swagger và health check
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

                var userId = context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                var log = new RequestLog
                {
                    IpAddress = ip,
                    UserId = userId,
                    Endpoint = path,
                    Method = context.Request.Method,
                    StatusCode = context.Response.StatusCode,
                    ResponseTimeMs = sw.ElapsedMilliseconds,
                    UserAgent = context.Request.Headers.UserAgent.ToString(),
                    Referer = context.Request.Headers.Referer.ToString(),
                    CreatedAt = DateTime.UtcNow
                };

                await dbContext.RequestLogs.AddAsync(log);
                await dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi ghi RequestLog");
            }
        }
    }
}

public static class RequestLoggingMiddlewareExtensions
{
    public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder app)
        => app.UseMiddleware<RequestLoggingMiddleware>();
}