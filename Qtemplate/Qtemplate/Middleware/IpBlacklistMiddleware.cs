using Microsoft.EntityFrameworkCore;
using Qtemplate.Infrastructure.Data;

namespace Qtemplate.Middleware;

public class IpBlacklistMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<IpBlacklistMiddleware> _logger;

    public IpBlacklistMiddleware(RequestDelegate next, ILogger<IpBlacklistMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, AppDbContext dbContext)
    {
        var ip = context.Request.Headers["X-Forwarded-For"].FirstOrDefault()
                 ?? context.Connection.RemoteIpAddress?.ToString()
                 ?? "unknown";

        var isBlocked = await dbContext.IpBlacklists
            .AnyAsync(b => b.IpAddress == ip
                        && b.IsActive
                        && (b.ExpiredAt == null || b.ExpiredAt > DateTime.UtcNow));

        if (isBlocked)
        {
            _logger.LogWarning("Chặn request từ IP bị blacklist: {Ip}", ip);
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsJsonAsync(new
            {
                success = false,
                message = "Địa chỉ IP của bạn đã bị chặn"
            });
            return;
        }

        await _next(context);
    }
}

public static class IpBlacklistMiddlewareExtensions
{
    public static IApplicationBuilder UseIpBlacklist(this IApplicationBuilder app)
        => app.UseMiddleware<IpBlacklistMiddleware>();
}