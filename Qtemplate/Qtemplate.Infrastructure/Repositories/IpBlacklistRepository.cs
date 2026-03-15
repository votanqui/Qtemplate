using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Qtemplate.Domain.Entities;
using Qtemplate.Domain.Interfaces.Repositories;
using Qtemplate.Infrastructure.Data;

namespace Qtemplate.Infrastructure.Repositories;

public class IpBlacklistRepository : IIpBlacklistRepository
{
    private readonly AppDbContext _db;
    private readonly IMemoryCache _cache;

    // Phải giống hệt CacheKey trong IpBlacklistMiddleware
    private const string CacheKey = "blocked_ips";

    public IpBlacklistRepository(AppDbContext db, IMemoryCache cache)
    {
        _db = db;
        _cache = cache;
    }

    public async Task<bool> IsBlockedAsync(string ipAddress)
    {
        var now = DateTime.UtcNow;
        return await _db.IpBlacklists.AnyAsync(x =>
            x.IpAddress == ipAddress &&
            x.IsActive &&
            (x.ExpiredAt == null || x.ExpiredAt > now));
    }

    public async Task<(List<IpBlacklist> Items, int Total)> GetPagedAsync(int page, int pageSize)
    {
        var query = _db.IpBlacklists.OrderByDescending(x => x.BlockedAt);
        var total = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        return (items, total);
    }

    public async Task<IpBlacklist?> GetByIdAsync(int id) =>
        await _db.IpBlacklists.FindAsync(id);

    public async Task<IpBlacklist?> GetByIpAsync(string ipAddress) =>
        await _db.IpBlacklists.FirstOrDefaultAsync(x => x.IpAddress == ipAddress);

    public async Task AddAsync(IpBlacklist entry)
    {
        await _db.IpBlacklists.AddAsync(entry);
        await _db.SaveChangesAsync();
        _cache.Remove(CacheKey);
    }

    public async Task UpdateAsync(IpBlacklist entry)
    {
        _db.IpBlacklists.Update(entry);
        await _db.SaveChangesAsync();
        _cache.Remove(CacheKey);
    }

    public async Task DeleteAsync(IpBlacklist entry)
    {
        _db.IpBlacklists.Remove(entry);
        await _db.SaveChangesAsync();
        _cache.Remove(CacheKey);
    }
}