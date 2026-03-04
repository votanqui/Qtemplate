using Microsoft.EntityFrameworkCore;
using Qtemplate.Domain.Entities;
using Qtemplate.Domain.Interfaces.Repositories;
using Qtemplate.Infrastructure.Data;

namespace Qtemplate.Infrastructure.Repositories;

public class WishlistRepository : IWishlistRepository
{
    private readonly AppDbContext _context;
    public WishlistRepository(AppDbContext context) => _context = context;

    public async Task<(List<Wishlist> Items, int Total)> GetPagedByUserIdAsync(Guid userId, int page, int pageSize)
    {
        var query = _context.Wishlists
            .Where(w => w.UserId == userId)
            .OrderByDescending(w => w.CreatedAt);

        var total = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Include(w => w.Template)
            .ToListAsync();

        return (items, total);
    }

    public async Task<Wishlist?> GetAsync(Guid userId, Guid templateId)
        => await _context.Wishlists
            .FirstOrDefaultAsync(w => w.UserId == userId && w.TemplateId == templateId);

    public async Task AddAsync(Wishlist wishlist)
    {
        await _context.Wishlists.AddAsync(wishlist);
        await _context.SaveChangesAsync();
    }

    public async Task RemoveAsync(Wishlist wishlist)
    {
        _context.Wishlists.Remove(wishlist);
        await _context.SaveChangesAsync();
    }
    public async Task<bool> ExistsAsync(Guid userId, Guid templateId) =>
    await _context.Wishlists.AnyAsync(w => w.UserId == userId && w.TemplateId == templateId);
    public async Task<List<Guid>> GetTemplateIdsByUserAsync(Guid userId) =>
    await _context.Wishlists
        .Where(w => w.UserId == userId)
        .Select(w => w.TemplateId)
        .ToListAsync();
    public async Task<(List<Wishlist> Items, int Total)> GetPagedAdminAsync(
    Guid? userId, Guid? templateId, int page, int pageSize)
    {
        var query = _context.Wishlists
            .Include(w => w.Template)
            .Include(w => w.User)
            .AsQueryable();

        if (userId.HasValue)
            query = query.Where(w => w.UserId == userId.Value);

        if (templateId.HasValue)
            query = query.Where(w => w.TemplateId == templateId.Value);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(w => w.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }

    public async Task<List<(Guid TemplateId, string TemplateName, int Count)>> GetTopWishlistedAsync(int top) =>
        await _context.Wishlists
            .GroupBy(w => new { w.TemplateId, w.Template.Name })
            .Select(g => new
            {
                g.Key.TemplateId,
                g.Key.Name,
                Count = g.Count()
            })
            .OrderByDescending(x => x.Count)
            .Take(top)
            .Select(x => ValueTuple.Create(x.TemplateId, x.Name, x.Count))
            .ToListAsync();
}