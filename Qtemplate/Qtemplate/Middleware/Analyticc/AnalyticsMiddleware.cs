// File: Qtemplate/Middleware/AnalyticsMiddleware.cs
//
// THAY ĐỔI SO VỚI BẢN CŨ:
//   - Bỏ hoàn toàn `await analyticsRepo.AddAsync(analytics)` — đây là nguyên nhân
//     gây DB write storm mỗi GET request.
//   - Thay bằng `_queue.Enqueue(analytics)` — non-blocking, O(1), không chờ DB.
//   - AnalyticsDrainService sẽ batch insert mỗi 5 giây.

using System.Security.Claims;
using Qtemplate.Domain.Entities;

namespace Qtemplate.Middleware.Analyticc;

public class AnalyticsMiddleware
{
    private readonly RequestDelegate _next;
    private readonly AnalyticsQueue _queue;

    // Chỉ track các route public có nghĩa
    private static readonly string[] _trackedPrefixes =
    [
        "/api/templates",
        "/api/categories",
        "/api/tags"
    ];

    // Bỏ qua admin, auth, swagger, static, và preview assets
    private static readonly string[] _excludedPrefixes =
    [
        "/api/admin",
        "/api/auth",
        "/api/preview",   // mỗi asset trong preview tạo 1 row analytics → spam
        "/swagger",
        "/health",
        "/favicon"
    ];

    public AnalyticsMiddleware(RequestDelegate next, AnalyticsQueue queue)
    {
        _next = next;
        _queue = queue;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        await _next(context);

        try
        {
            var path = context.Request.Path.Value ?? string.Empty;
            var method = context.Request.Method;

            // Chỉ track GET requests
            if (method != "GET") return;

            // Bỏ qua excluded
            if (_excludedPrefixes.Any(p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
                return;

            // Chỉ track các prefix có nghĩa
            if (!_trackedPrefixes.Any(p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
                return;

            var query = context.Request.QueryString.Value ?? string.Empty;
            var userAgent = context.Request.Headers.UserAgent.ToString();
            var referer = context.Request.Headers.Referer.ToString();
            var ip = context.Request.Headers["X-Forwarded-For"].FirstOrDefault()
                            ?? context.Connection.RemoteIpAddress?.ToString()
                            ?? "unknown";

            var userId = context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            var sessionId = context.Request.Cookies["sessionId"]
                            ?? context.Request.Headers["X-Session-Id"].FirstOrDefault();

            // Parse UTM từ query string
            var qs = System.Web.HttpUtility.ParseQueryString(query);

            var analytics = new Analytics
            {
                SessionId = sessionId,
                UserId = userId,
                IpAddress = ip,
                Device = DetectDevice(userAgent),
                Browser = DetectBrowser(userAgent),
                OS = DetectOS(userAgent),
                PageUrl = path + query,
                Referer = string.IsNullOrEmpty(referer) ? null : referer,
                UTMSource = qs["utm_source"],
                UTMMedium = qs["utm_medium"],
                UTMCampaign = qs["utm_campaign"],
                AffiliateCode = qs["ref"] ?? qs["aff"],
                CreatedAt = DateTime.UtcNow
            };

            // NON-BLOCKING: đẩy vào queue, AnalyticsDrainService sẽ batch insert
            // Nếu queue đầy (50k items) → DropOldest, không bao giờ block request
            _queue.Enqueue(analytics);
        }
        catch
        {
            // Không để analytics lỗi ảnh hưởng request
        }
    }

    private static string DetectDevice(string ua)
    {
        if (string.IsNullOrEmpty(ua)) return "Unknown";
        if (ua.Contains("Mobile") || ua.Contains("Android") && !ua.Contains("Tablet"))
            return "Mobile";
        if (ua.Contains("Tablet") || ua.Contains("iPad"))
            return "Tablet";
        return "Desktop";
    }

    private static string DetectBrowser(string ua)
    {
        if (string.IsNullOrEmpty(ua)) return "Unknown";
        if (ua.Contains("Edg/")) return "Edge";
        if (ua.Contains("OPR/")) return "Opera";
        if (ua.Contains("Chrome/")) return "Chrome";
        if (ua.Contains("Firefox/")) return "Firefox";
        if (ua.Contains("Safari/") && !ua.Contains("Chrome")) return "Safari";
        return "Other";
    }

    private static string DetectOS(string ua)
    {
        if (string.IsNullOrEmpty(ua)) return "Unknown";
        if (ua.Contains("Windows NT")) return "Windows";
        if (ua.Contains("Mac OS X")) return "macOS";
        if (ua.Contains("Android")) return "Android";
        if (ua.Contains("iPhone") || ua.Contains("iPad")) return "iOS";
        if (ua.Contains("Linux")) return "Linux";
        return "Other";
    }
}

public static class AnalyticsMiddlewareExtensions
{
    public static IApplicationBuilder UseAnalyticsTracking(this IApplicationBuilder app)
        => app.UseMiddleware<AnalyticsMiddleware>();
}