using Microsoft.EntityFrameworkCore;
using Qtemplate.Domain.Entities;
using Qtemplate.Domain.Interfaces.Repositories;
using Qtemplate.Infrastructure.Data;

namespace Qtemplate.Infrastructure.Repositories;

public class RequestLogRepository : IRequestLogRepository
{
    private readonly AppDbContext _db;
    public RequestLogRepository(AppDbContext db) => _db = db;

    public async Task AddAsync(RequestLog log)
    {
        await _db.RequestLogs.AddAsync(log);
        await _db.SaveChangesAsync();
    }

    public async Task<(List<RequestLog> Items, int Total)> GetPagedAsync(
        string? ip, string? userId, string? endpoint,
        int? statusCode, int page, int pageSize)
    {
        var query = _db.RequestLogs.AsQueryable();

        if (!string.IsNullOrEmpty(ip))
            query = query.Where(r => r.IpAddress.Contains(ip));

        if (!string.IsNullOrEmpty(userId))
            query = query.Where(r => r.UserId == userId);

        if (!string.IsNullOrEmpty(endpoint))
            query = query.Where(r => r.Endpoint.Contains(endpoint));

        if (statusCode.HasValue)
            query = query.Where(r => r.StatusCode == statusCode.Value);

        query = query.OrderByDescending(r => r.CreatedAt);

        var total = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }

    // ── Security scanner ─────────────────────────────────────────────────────

    public async Task<List<(string IpAddress, string? UserId, int Count)>> GetHighVolumeAsync(
        DateTime from, int threshold)
    {
        var rows = await _db.RequestLogs
            .Where(r => r.CreatedAt >= from)
            .GroupBy(r => new { r.IpAddress, r.UserId })
            .Where(g => g.Count() >= threshold)
            .Select(g => new { g.Key.IpAddress, g.Key.UserId, Count = g.Count() })
            .ToListAsync();

        return rows.Select(x => (x.IpAddress, x.UserId, x.Count)).ToList();
    }

    public async Task<List<(string IpAddress, string? UserId, int ErrorPercent)>> GetHighErrorRateAsync(
        DateTime from, int minTotal, int errorPctThreshold)
    {
        var rows = await _db.RequestLogs
            .Where(r => r.CreatedAt >= from)
            .GroupBy(r => new { r.IpAddress, r.UserId })
            .Select(g => new
            {
                g.Key.IpAddress,
                g.Key.UserId,
                Total = g.Count(),
                Errors = g.Count(r => r.StatusCode >= 400)
            })
            .Where(x => x.Total >= minTotal)
            .ToListAsync();

        return rows
            .Where(x => x.Errors * 100 / x.Total >= errorPctThreshold)
            .Select(x => (x.IpAddress, x.UserId, x.Errors * 100 / x.Total))
            .ToList();
    }

    public async Task<List<(string IpAddress, string? UserId, int Count)>> GetEndpointScanAsync(
        DateTime from, int threshold)
    {
        var rows = await _db.RequestLogs
            .Where(r => r.CreatedAt >= from && r.StatusCode == 404)
            .GroupBy(r => new { r.IpAddress, r.UserId })
            .Where(g => g.Count() >= threshold)
            .Select(g => new { g.Key.IpAddress, g.Key.UserId, Count = g.Count() })
            .ToListAsync();

        return rows.Select(x => (x.IpAddress, x.UserId, x.Count)).ToList();
    }
}