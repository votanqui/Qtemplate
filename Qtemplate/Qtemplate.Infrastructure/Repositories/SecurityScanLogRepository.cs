using Microsoft.EntityFrameworkCore;
using Qtemplate.Domain.Entities;
using Qtemplate.Domain.Interfaces.Repositories;
using Qtemplate.Infrastructure.Data;

namespace Qtemplate.Infrastructure.Repositories;

public class SecurityScanLogRepository : ISecurityScanLogRepository
{
    private readonly AppDbContext _db;
    public SecurityScanLogRepository(AppDbContext db) => _db = db;

    public async Task AddAsync(SecurityScanLog log)
    {
        await _db.SecurityScanLogs.AddAsync(log);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(SecurityScanLog log)
    {
        _db.SecurityScanLogs.Update(log);
        await _db.SaveChangesAsync();
    }

    public async Task<SecurityScanLog?> GetByIdAsync(long id) =>
        await _db.SecurityScanLogs.FindAsync(id);

    public async Task<bool> IsAlreadyHandledAsync(
        string violation, string? ipAddress, Guid? userId, DateTime windowFrom)
    {
        // Tìm bản ghi đã xử lý cùng violation + cùng IP/User trong cửa sổ thời gian
        // Nếu tìm thấy VÀ IsAdminOverride = false → scanner đã xử lý rồi, bỏ qua
        // Nếu IsAdminOverride = true → admin đã mở khoá, scanner cũng bỏ qua (tôn trọng quyết định admin)
        return await _db.SecurityScanLogs.AnyAsync(l =>
            l.Violation == violation &&
            l.ScannedAt >= windowFrom &&
            (ipAddress == null || l.IpAddress == ipAddress) &&
            (userId == null || l.UserId == userId));
    }

    public async Task<(List<SecurityScanLog> Items, int Total)> GetPagedAsync(
        string? violation, Guid? userId, string? ipAddress,
        bool? isOverride, int page, int pageSize)
    {
        var query = _db.SecurityScanLogs.AsQueryable();

        if (!string.IsNullOrWhiteSpace(violation))
            query = query.Where(l => l.Violation == violation);
        if (userId.HasValue)
            query = query.Where(l => l.UserId == userId);
        if (!string.IsNullOrWhiteSpace(ipAddress))
            query = query.Where(l => l.IpAddress != null && l.IpAddress.Contains(ipAddress));
        if (isOverride.HasValue)
            query = query.Where(l => l.IsAdminOverride == isOverride.Value);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(l => l.ScannedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }
}