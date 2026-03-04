using Microsoft.EntityFrameworkCore;
using Qtemplate.Domain.Entities;
using Qtemplate.Domain.Interfaces.Repositories;
using Qtemplate.Infrastructure.Data;

namespace Qtemplate.Infrastructure.Repositories;

public class BannerRepository : IBannerRepository
{
    private readonly AppDbContext _db;
    public BannerRepository(AppDbContext db) => _db = db;

    public async Task<List<Banner>> GetActiveByPositionAsync(string? position)
    {
        var now = DateTime.UtcNow;
        var query = _db.Banners
            .Where(b => b.IsActive
                && (b.StartAt == null || b.StartAt <= now)
                && (b.EndAt == null || b.EndAt >= now));

        if (!string.IsNullOrEmpty(position))
            query = query.Where(b => b.Position == position);

        return await query
            .OrderBy(b => b.SortOrder)
            .ToListAsync();
    }

    public async Task<(List<Banner> Items, int Total)> GetAdminListAsync(int page, int pageSize)
    {
        var query = _db.Banners.OrderBy(b => b.Position).ThenBy(b => b.SortOrder);
        var total = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        return (items, total);
    }

    public async Task<Banner?> GetByIdAsync(int id) =>
        await _db.Banners.FindAsync(id);

    public async Task AddAsync(Banner banner)
    {
        await _db.Banners.AddAsync(banner);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Banner banner)
    {
        _db.Banners.Update(banner);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(Banner banner)
    {
        _db.Banners.Remove(banner);
        await _db.SaveChangesAsync();
    }
}