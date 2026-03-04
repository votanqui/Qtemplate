using Microsoft.EntityFrameworkCore;
using Qtemplate.Domain.Entities;
using Qtemplate.Domain.Interfaces.Repositories;
using Qtemplate.Infrastructure.Data;

namespace Qtemplate.Infrastructure.Repositories;

public class StatsRepository : IStatsRepository
{
    private readonly AppDbContext _db;
    public StatsRepository(AppDbContext db) => _db = db;

    // ── Order ─────────────────────────────────────────────────────────────────
    public async Task<List<Order>> GetOrdersInRangeAsync(
        DateTime from, DateTime to, bool includeItems = false)
    {
        var query = _db.Orders
            .Include(o => o.User)
            .AsQueryable();

        if (includeItems)
            query = query.Include(o => o.Items).ThenInclude(i => i.Template);

        return await query
            .Where(o => o.CreatedAt >= from && o.CreatedAt <= to)
            .ToListAsync();
    }

    public async Task<List<Order>> GetPaidOrdersAsync() =>
        await _db.Orders
            .Where(o => o.Status == "Paid" || o.Status == "Completed")
            .ToListAsync();

    // ── Payment ───────────────────────────────────────────────────────────────
    public async Task<List<Payment>> GetPaymentsInRangeAsync(
        DateTime from, DateTime to, bool includeOrder = false)
    {
        var query = _db.Payments.AsQueryable();
        if (includeOrder)
            query = query.Include(p => p.Order);
        return await query
            .Where(p => p.CreatedAt >= from && p.CreatedAt <= to)
            .ToListAsync();
    }

    // ── Coupon ────────────────────────────────────────────────────────────────
    public async Task<List<Coupon>> GetAllCouponsAsync() =>
        await _db.Coupons.ToListAsync();

    public async Task<List<(string Code, decimal TotalDiscount)>> GetCouponUsageAsync()
    {
        var raw = await _db.Orders
            .Where(o => o.Status == "Paid" && o.CouponCode != null)
            .GroupBy(o => o.CouponCode!)
            .Select(g => new { Code = g.Key, TotalDiscount = g.Sum(o => o.DiscountAmount) })
            .ToListAsync();
        return raw.Select(x => (x.Code, x.TotalDiscount)).ToList();
    }

    // ── Analytics ─────────────────────────────────────────────────────────────
    public async Task<List<Analytics>> GetAnalyticsInRangeAsync(DateTime from, DateTime to) =>
        await _db.Analytics
            .Where(a => a.CreatedAt >= from && a.CreatedAt <= to)
            .ToListAsync();

    // ── IpBlacklist ───────────────────────────────────────────────────────────
    public async Task<List<IpBlacklist>> GetIpBlacklistPagedAsync(int page, int pageSize) =>
        await _db.IpBlacklists
            .OrderByDescending(x => x.BlockedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

    public async Task<int> CountIpBlacklistAsync() =>
        await _db.IpBlacklists.CountAsync();

    // ── RequestLog ────────────────────────────────────────────────────────────
    public async Task<List<RequestLog>> GetRequestLogsPagedAsync(
        string? ip, string? userId, string? endpoint, int? statusCode,
        int page, int pageSize)
    {
        var query = BuildRequestLogQuery(ip, userId, endpoint, statusCode);
        return await query
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> CountRequestLogsAsync(
        string? ip, string? userId, string? endpoint, int? statusCode) =>
        await BuildRequestLogQuery(ip, userId, endpoint, statusCode).CountAsync();

    private IQueryable<RequestLog> BuildRequestLogQuery(
        string? ip, string? userId, string? endpoint, int? statusCode)
    {
        var query = _db.RequestLogs.AsQueryable();
        if (!string.IsNullOrEmpty(ip)) query = query.Where(r => r.IpAddress.Contains(ip));
        if (!string.IsNullOrEmpty(userId)) query = query.Where(r => r.UserId == userId);
        if (!string.IsNullOrEmpty(endpoint)) query = query.Where(r => r.Endpoint.Contains(endpoint));
        if (statusCode.HasValue) query = query.Where(r => r.StatusCode == statusCode.Value);
        return query;
    }

    // ── EmailLog ──────────────────────────────────────────────────────────────
    public async Task<List<EmailLog>> GetEmailLogsPagedAsync(
        string? status, string? template, int page, int pageSize)
    {
        var query = BuildEmailLogQuery(status, template);
        return await query
            .OrderByDescending(e => e.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> CountEmailLogsAsync(string? status, string? template) =>
        await BuildEmailLogQuery(status, template).CountAsync();

    private IQueryable<EmailLog> BuildEmailLogQuery(string? status, string? template)
    {
        var query = _db.EmailLogs.AsQueryable();
        if (!string.IsNullOrEmpty(status)) query = query.Where(e => e.Status == status);
        if (!string.IsNullOrEmpty(template)) query = query.Where(e => e.Template == template);
        return query;
    }
    public async Task<List<IpBlacklist>> GetAllIpBlacklistAsync() =>
    await _db.IpBlacklists
        .OrderByDescending(x => x.BlockedAt)
        .ToListAsync();

    public async Task<List<RequestLog>> GetRequestLogsInRangeAsync(DateTime from, DateTime to) =>
        await _db.RequestLogs
            .Where(r => r.CreatedAt >= from && r.CreatedAt <= to)
            .ToListAsync();

    public async Task<List<EmailLog>> GetAllEmailLogsAsync() =>
        await _db.EmailLogs
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync();
}