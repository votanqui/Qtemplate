// File: Qtemplate.Infrastructure/Repositories/StatsRepository.cs
//
// THAY ĐỔI SO VỚI BẢN CŨ:
//   1. GetPaidOrdersAsync() → thêm tham số from/to, dùng AsNoTracking()
//   2. GetAnalyticsInRangeAsync() → vẫn load entities nhưng thêm AsNoTracking()
//      (không thể projection ở đây vì handler dùng toàn bộ fields)
//   3. GetOrdersInRangeAsync() → thêm AsNoTracking()
//   4. GetPaymentsInRangeAsync() → thêm AsNoTracking()
//   5. GetAllCouponsAsync() → thêm AsNoTracking()
//   6. GetRequestLogsInRangeAsync() → thêm AsNoTracking() + giới hạn 10k rows
//   7. GetAllEmailLogsAsync() → thêm AsNoTracking() + giới hạn 5k rows
//   8. GetAllIpBlacklistAsync() → thêm AsNoTracking()

using Microsoft.EntityFrameworkCore;
using Qtemplate.Domain.Entities;
using Qtemplate.Domain.Interfaces.Repositories;
using Qtemplate.Infrastructure.Data;

namespace Qtemplate.Infrastructure.Repositories;

public class StatsRepository : IStatsRepository
{
    private readonly AppDbContext _db;
    public StatsRepository(AppDbContext db) => _db = db;

    // ── Order ──────────────────────────────────────────────────────────────────
    public async Task<List<Order>> GetOrdersInRangeAsync(
        DateTime from, DateTime to, bool includeItems = false)
    {
        var query = _db.Orders
            .Include(o => o.User)
            .AsNoTracking()   // stats chỉ đọc, không cần track
            .AsQueryable();

        if (includeItems)
            query = query.Include(o => o.Items).ThenInclude(i => i.Template);

        return await query
            .Where(o => o.CreatedAt >= from && o.CreatedAt <= to)
            .ToListAsync();
    }

    // THAY ĐỔI: thêm from/to để không load toàn bộ bảng Orders
    // Handler cũ gọi GetPaidOrdersAsync() không có tham số → load tất cả → OOM
    // Interface cũng được cập nhật tương ứng (xem IStatsRepository.cs)
    public async Task<List<Order>> GetPaidOrdersAsync(DateTime? from = null, DateTime? to = null)
    {
        var query = _db.Orders
            .Where(o => o.Status == "Paid" || o.Status == "Completed")
            .AsNoTracking()
            .AsQueryable();

        if (from.HasValue) query = query.Where(o => o.CreatedAt >= from.Value);
        if (to.HasValue) query = query.Where(o => o.CreatedAt <= to.Value);

        return await query.ToListAsync();
    }

    // ── Payment ────────────────────────────────────────────────────────────────
    public async Task<List<Payment>> GetPaymentsInRangeAsync(
        DateTime from, DateTime to, bool includeOrder = false)
    {
        var query = _db.Payments
            .AsNoTracking()
            .AsQueryable();

        if (includeOrder) query = query.Include(p => p.Order);

        return await query
            .Where(p => p.CreatedAt >= from && p.CreatedAt <= to)
            .ToListAsync();
    }

    // ── Coupon ─────────────────────────────────────────────────────────────────
    public async Task<List<Coupon>> GetAllCouponsAsync() =>
        await _db.Coupons
            .AsNoTracking()
            .ToListAsync();

    public async Task<List<(string Code, decimal TotalDiscount)>> GetCouponUsageAsync()
    {
        var raw = await _db.Orders
            .Where(o => o.Status == "Paid" && o.CouponCode != null)
            .GroupBy(o => o.CouponCode!)
            .Select(g => new { Code = g.Key, TotalDiscount = g.Sum(o => o.DiscountAmount) })
            .AsNoTracking()
            .ToListAsync();
        return raw.Select(x => (x.Code, x.TotalDiscount)).ToList();
    }

    // ── Analytics ──────────────────────────────────────────────────────────────
    // AsNoTracking() — handler chỉ đọc để tính thống kê, không update
    public async Task<List<Analytics>> GetAnalyticsInRangeAsync(DateTime from, DateTime to) =>
        await _db.Analytics
            .Where(a => a.CreatedAt >= from && a.CreatedAt <= to)
            .AsNoTracking()
            .ToListAsync();

    // ── IpBlacklist ────────────────────────────────────────────────────────────
    public async Task<List<IpBlacklist>> GetIpBlacklistPagedAsync(int page, int pageSize) =>
        await _db.IpBlacklists
            .OrderByDescending(x => x.BlockedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync();

    public async Task<int> CountIpBlacklistAsync() =>
        await _db.IpBlacklists.CountAsync();

    // ── RequestLog ─────────────────────────────────────────────────────────────
    public async Task<List<RequestLog>> GetRequestLogsPagedAsync(
        string? ip, string? userId, string? endpoint, int? statusCode, int page, int pageSize) =>
        await BuildRequestLogQuery(ip, userId, endpoint, statusCode)
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync();

    public async Task<int> CountRequestLogsAsync(
        string? ip, string? userId, string? endpoint, int? statusCode) =>
        await BuildRequestLogQuery(ip, userId, endpoint, statusCode).CountAsync();

    private IQueryable<RequestLog> BuildRequestLogQuery(
        string? ip, string? userId, string? endpoint, int? statusCode)
    {
        var q = _db.RequestLogs.AsQueryable();
        if (!string.IsNullOrEmpty(ip)) q = q.Where(r => r.IpAddress.Contains(ip));
        if (!string.IsNullOrEmpty(userId)) q = q.Where(r => r.UserId == userId);
        if (!string.IsNullOrEmpty(endpoint)) q = q.Where(r => r.Endpoint.Contains(endpoint));
        if (statusCode.HasValue) q = q.Where(r => r.StatusCode == statusCode.Value);
        return q;
    }

    // ── EmailLog ───────────────────────────────────────────────────────────────
    public async Task<List<EmailLog>> GetEmailLogsPagedAsync(
        string? status, string? template, int page, int pageSize) =>
        await BuildEmailLogQuery(status, template)
            .OrderByDescending(e => e.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync();

    public async Task<int> CountEmailLogsAsync(string? status, string? template) =>
        await BuildEmailLogQuery(status, template).CountAsync();

    private IQueryable<EmailLog> BuildEmailLogQuery(string? status, string? template)
    {
        var q = _db.EmailLogs.AsQueryable();
        if (!string.IsNullOrEmpty(status)) q = q.Where(e => e.Status == status);
        if (!string.IsNullOrEmpty(template)) q = q.Where(e => e.Template == template);
        return q;
    }

    public async Task<List<IpBlacklist>> GetAllIpBlacklistAsync() =>
        await _db.IpBlacklists
            .OrderByDescending(x => x.BlockedAt)
            .AsNoTracking()
            .ToListAsync();

    // THAY ĐỔI: giới hạn 10_000 rows — không load toàn bộ bảng vào RAM
    public async Task<List<RequestLog>> GetRequestLogsInRangeAsync(DateTime from, DateTime to) =>
        await _db.RequestLogs
            .Where(r => r.CreatedAt >= from && r.CreatedAt <= to)
            .OrderByDescending(r => r.CreatedAt)
            .Take(10_000)
            .AsNoTracking()
            .ToListAsync();

    // THAY ĐỔI: giới hạn 5_000 rows
    public async Task<List<EmailLog>> GetAllEmailLogsAsync() =>
        await _db.EmailLogs
            .OrderByDescending(e => e.CreatedAt)
            .Take(5_000)
            .AsNoTracking()
            .ToListAsync();

    // ── Daily Stats ────────────────────────────────────────────────────────────
    public async Task<List<DailyStat>> GetDailyStatsAsync(DateTime from, DateTime to) =>
        await _db.DailyStats
            .Where(s => s.Date >= from && s.Date <= to)
            .OrderBy(s => s.Date)
            .AsNoTracking()
            .ToListAsync();
}