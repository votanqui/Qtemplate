using Microsoft.EntityFrameworkCore;
using Qtemplate.Domain.Entities;
using Qtemplate.Domain.Interfaces.Repositories;
using Qtemplate.Infrastructure.Data;

namespace Qtemplate.Infrastructure.Repositories;

public class AuditLogRepository : IAuditLogRepository
{
    private readonly AppDbContext _db;
    public AuditLogRepository(AppDbContext db) => _db = db;

    public async Task<(List<AuditLog> Items, int Total)> GetPagedAsync(
        string? userEmail,
        string? action,
        string? entityName,
        string? entityId,
        DateTime? from,
        DateTime? to,
        int page,
        int pageSize)
    {
        var q = _db.AuditLogs.AsQueryable();

        if (!string.IsNullOrEmpty(userEmail))
            q = q.Where(a => a.UserEmail != null && a.UserEmail.Contains(userEmail));

        if (!string.IsNullOrEmpty(action))
            q = q.Where(a => a.Action == action);

        if (!string.IsNullOrEmpty(entityName))
            q = q.Where(a => a.EntityName == entityName);

        if (!string.IsNullOrEmpty(entityId))
            q = q.Where(a => a.EntityId == entityId);

        if (from.HasValue)
            q = q.Where(a => a.CreatedAt >= from.Value);

        if (to.HasValue)
            q = q.Where(a => a.CreatedAt <= to.Value.AddDays(1));

        q = q.OrderByDescending(a => a.CreatedAt);

        var total = await q.CountAsync();
        var items = await q
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }
}