using Microsoft.EntityFrameworkCore;
using Qtemplate.Domain.Entities;
using Qtemplate.Domain.Interfaces.Repositories;
using Qtemplate.Infrastructure.Data;

namespace Qtemplate.Infrastructure.Repositories;

public class PostRepository : IPostRepository
{
    private readonly AppDbContext _db;
    public PostRepository(AppDbContext db) => _db = db;

    // ── Public ───────────────────────────────────────────────────────────────

    public async Task<(List<Post> Items, int Total)> GetPublishedAsync(
        int page, int pageSize, string? search, bool? isFeatured)
    {
        var query = _db.Posts
            .Where(p => p.Status == "Published")
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(p =>
                p.Title.Contains(search) ||
                (p.Excerpt != null && p.Excerpt.Contains(search)));

        if (isFeatured.HasValue)
            query = query.Where(p => p.IsFeatured == isFeatured.Value);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(p => p.IsFeatured)   // featured lên trước
            .ThenByDescending(p => p.PublishedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }

    public async Task<Post?> GetBySlugAsync(string slug) =>
        await _db.Posts.FirstOrDefaultAsync(p => p.Slug == slug);

    public async Task IncrementViewCountAsync(int id)
    {
        await _db.Posts
            .Where(p => p.Id == id)
            .ExecuteUpdateAsync(s => s.SetProperty(p => p.ViewCount, p => p.ViewCount + 1));
    }

    // ── Admin ────────────────────────────────────────────────────────────────

    public async Task<(List<Post> Items, int Total)> GetAdminListAsync(
        int page, int pageSize, string? search, string? status)
    {
        var query = _db.Posts.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(p =>
                p.Title.Contains(search) ||
                p.AuthorName.Contains(search));

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(p => p.Status == status);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(p => p.UpdatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }

    public async Task<Post?> GetByIdAsync(int id) =>
        await _db.Posts.FindAsync(id);

    public async Task AddAsync(Post post)
    {
        await _db.Posts.AddAsync(post);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Post post)
    {
        _db.Posts.Update(post);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(Post post)
    {
        _db.Posts.Remove(post);
        await _db.SaveChangesAsync();
    }

    public async Task<bool> SlugExistsAsync(string slug, int? excludeId = null)
    {
        var query = _db.Posts.Where(p => p.Slug == slug);
        if (excludeId.HasValue)
            query = query.Where(p => p.Id != excludeId.Value);
        return await query.AnyAsync();
    }
}