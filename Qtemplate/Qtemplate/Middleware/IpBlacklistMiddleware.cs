using Microsoft.Extensions.Caching.Memory;
using Qtemplate.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Qtemplate.Middleware;

public class IpBlacklistMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<IpBlacklistMiddleware> _logger;

    private const string CacheKey = "blocked_ips";
    private static readonly TimeSpan CacheTtl = TimeSpan.FromSeconds(60);

    private static readonly SemaphoreSlim _refreshLock = new(1, 1);

    public IpBlacklistMiddleware(RequestDelegate next, ILogger<IpBlacklistMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, AppDbContext dbContext, IMemoryCache cache)
    {
        var ip = context.Request.Headers["X-Forwarded-For"].FirstOrDefault()
                 ?? context.Connection.RemoteIpAddress?.ToString()
                 ?? "unknown";

        if (await IsBlockedAsync(ip, dbContext, cache))
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

    private static async Task<bool> IsBlockedAsync(string ip, AppDbContext db, IMemoryCache cache)
    {
        // Fast path: cache hit — không lock, không DB, O(1)
        if (cache.TryGetValue(CacheKey, out HashSet<string>? cached) && cached is not null)
            return cached.Contains(ip);

        // Slow path: cache miss — chỉ 1 thread được refresh, còn lại chờ
        await _refreshLock.WaitAsync();
        try
        {
            // Double-check sau khi acquire lock — có thể thread khác đã refresh rồi
            if (cache.TryGetValue(CacheKey, out cached) && cached is not null)
                return cached.Contains(ip);

            // Thực sự query DB
            var now = DateTime.UtcNow;
            var ips = await db.IpBlacklists
                .Where(b => b.IsActive && (b.ExpiredAt == null || b.ExpiredAt > now))
                .Select(b => b.IpAddress)
                .AsNoTracking()
                .ToListAsync();

            var ipSet = new HashSet<string>(ips, StringComparer.OrdinalIgnoreCase);

            cache.Set(CacheKey, ipSet, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = CacheTtl,
                Size = 1
            });

            return ipSet.Contains(ip);
        }
        finally
        {
            _refreshLock.Release();
        }
    }
}

public static class IpBlacklistCache
{
    public static void Invalidate(IMemoryCache cache) => cache.Remove("blocked_ips");
}

public static class IpBlacklistMiddlewareExtensions
{
    public static IApplicationBuilder UseIpBlacklist(this IApplicationBuilder app)
        => app.UseMiddleware<IpBlacklistMiddleware>();
}