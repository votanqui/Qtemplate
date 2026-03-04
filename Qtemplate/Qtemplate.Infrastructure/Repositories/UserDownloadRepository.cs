using Microsoft.EntityFrameworkCore;
using Qtemplate.Domain.Entities;
using Qtemplate.Domain.Interfaces.Repositories;
using Qtemplate.Infrastructure.Data;

namespace Qtemplate.Infrastructure.Repositories;

public class UserDownloadRepository : IUserDownloadRepository
{
    private readonly AppDbContext _db;
    public UserDownloadRepository(AppDbContext db) => _db = db;

    public async Task<(List<UserDownload>, int)> GetPagedByUserIdAsync(
        Guid userId, int page, int pageSize)
    {
        var query = _db.UserDownloads
            .Include(d => d.Template)
            .Where(d => d.UserId == userId)
            .OrderByDescending(d => d.LastDownloadAt);

        var total = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }

    public async Task UpsertAsync(Guid userId, Guid templateId, Guid orderId,
        string? ip = null, string? userAgent = null)
    {
        var existing = await _db.UserDownloads
            .FirstOrDefaultAsync(d => d.UserId == userId && d.TemplateId == templateId);

        var device = ParseDevice(userAgent);
        var browser = ParseBrowser(userAgent);
        var os = ParseOS(userAgent);

        if (existing is null)
        {
            await _db.UserDownloads.AddAsync(new UserDownload
            {
                UserId = userId,
                TemplateId = templateId,
                OrderId = orderId,
                IpAddress = ip,
                UserAgent = userAgent,
                Device = device,
                Browser = browser,
                OS = os,
                DownloadCount = 1,
                LastDownloadAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            });
        }
        else
        {
            existing.DownloadCount++;
            existing.LastDownloadAt = DateTime.UtcNow;
            existing.IpAddress = ip ?? existing.IpAddress;
            existing.UserAgent = userAgent ?? existing.UserAgent;
            existing.Device = device ?? existing.Device;
            existing.Browser = browser ?? existing.Browser;
            existing.OS = os ?? existing.OS;

            if (existing.OrderId == Guid.Empty && orderId != Guid.Empty)
                existing.OrderId = orderId;
        }

        await _db.SaveChangesAsync();
    }

    private static string? ParseDevice(string? ua)
    {
        if (string.IsNullOrEmpty(ua)) return null;
        if (ua.Contains("Mobile") || (ua.Contains("Android") && !ua.Contains("Tablet")))
            return "Mobile";
        if (ua.Contains("Tablet") || ua.Contains("iPad"))
            return "Tablet";
        return "Desktop";
    }

    private static string? ParseBrowser(string? ua)
    {
        if (string.IsNullOrEmpty(ua)) return null;
        if (ua.Contains("Edg/")) return "Edge";
        if (ua.Contains("OPR/")) return "Opera";
        if (ua.Contains("Chrome/")) return "Chrome";
        if (ua.Contains("Firefox/")) return "Firefox";
        if (ua.Contains("Safari/") && !ua.Contains("Chrome")) return "Safari";
        return "Other";
    }

    private static string? ParseOS(string? ua)
    {
        if (string.IsNullOrEmpty(ua)) return null;
        if (ua.Contains("Windows NT")) return "Windows";
        if (ua.Contains("Mac OS X")) return "macOS";
        if (ua.Contains("Android")) return "Android";
        if (ua.Contains("iPhone") || ua.Contains("iPad")) return "iOS";
        if (ua.Contains("Linux")) return "Linux";
        return "Other";
    }
}