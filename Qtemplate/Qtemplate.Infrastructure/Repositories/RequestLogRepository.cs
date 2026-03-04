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
}