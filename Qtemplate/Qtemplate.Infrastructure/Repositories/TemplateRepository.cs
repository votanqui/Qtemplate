using Microsoft.EntityFrameworkCore;
using Qtemplate.Domain.Entities;
using Qtemplate.Domain.Interfaces.Repositories;
using Qtemplate.Infrastructure.Data;

namespace Qtemplate.Infrastructure.Repositories;

public class TemplateRepository : ITemplateRepository
{
    private readonly AppDbContext _context;
    public TemplateRepository(AppDbContext context) => _context = context;

    public async Task<Template?> GetByIdAsync(Guid id)
        => await _context.Templates.FindAsync(id);

    public async Task UpdateAsync(Template template)
    {
        _context.Templates.Update(template);
        await _context.SaveChangesAsync();
    }
    public async Task<bool> SlugExistsAsync(string slug) =>
    await _context.Templates.AnyAsync(t => t.Slug == slug);

    public async Task<Template?> GetByIdWithDetailsAsync(Guid id) =>
        await _context.Templates
            .Include(t => t.TemplateTags)
            .Include(t => t.Features)
            .FirstOrDefaultAsync(t => t.Id == id);

    public async Task<(List<Template> Items, int Total)> GetAdminListAsync(
        string? search, string? status, int? categoryId, int page, int pageSize)
    {
        var query = _context.Templates
            .Include(t => t.Category)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(t => t.Name.Contains(search) || t.Slug.Contains(search));

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(t => t.Status == status);

        if (categoryId.HasValue)
            query = query.Where(t => t.CategoryId == categoryId);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }

    public async Task AddAsync(Template template)
    {
        await _context.Templates.AddAsync(template);
        await _context.SaveChangesAsync();
    }


    public async Task<Template?> GetByIdFullAsync(Guid id) =>
        await _context.Templates
            .Include(t => t.Images)
            .Include(t => t.TemplateTags)
            .Include(t => t.Features)
            .Include(t => t.Reviews)
            .Include(t => t.Wishlists)
            .FirstOrDefaultAsync(t => t.Id == id);


    public async Task DeleteAsync(Template template)
    {
        await _context.Templates
            .Where(t => t.Id == template.Id)
            .ExecuteDeleteAsync();
    }
    public async Task<Template?> GetBySlugAsync(string slug) =>
        await _context.Templates
            .Include(t => t.Category)
            .Include(t => t.TemplateTags).ThenInclude(tt => tt.Tag)
            .Include(t => t.Features)
            .Include(t => t.Images)
            .FirstOrDefaultAsync(t => t.Slug == slug);

    public async Task IncrementViewCountAsync(Guid id) =>
        await _context.Templates
            .Where(t => t.Id == id)
            .ExecuteUpdateAsync(s => s.SetProperty(t => t.ViewCount, t => t.ViewCount + 1));

    public async Task<(List<Template> Items, int Total)> GetPublicListAsync(
    string? search, string? categorySlug, string? tagSlug,
    bool? isFree, decimal? minPrice, decimal? maxPrice,
    string sortBy, int page, int pageSize)
    {
        var query = _context.Templates
            .Include(t => t.Category)
            .Include(t => t.TemplateTags).ThenInclude(tt => tt.Tag)
            .Where(t => t.Status == "Published")
            .AsQueryable();

        if (!string.IsNullOrEmpty(search))
            query = query.Where(t =>
                t.Name.Contains(search) ||
                (t.ShortDescription != null && t.ShortDescription.Contains(search)) ||
                (t.TechStack != null && t.TechStack.Contains(search)));

        if (!string.IsNullOrEmpty(categorySlug))
            query = query.Where(t => t.Category.Slug == categorySlug);

        if (!string.IsNullOrEmpty(tagSlug))
            query = query.Where(t => t.TemplateTags.Any(tt => tt.Tag.Slug == tagSlug));

        if (isFree.HasValue)
            query = query.Where(t => t.IsFree == isFree.Value);

        if (minPrice.HasValue)
            query = query.Where(t => (t.SalePrice ?? t.Price) >= minPrice.Value);

        if (maxPrice.HasValue)
            query = query.Where(t => (t.SalePrice ?? t.Price) <= maxPrice.Value);

        query = sortBy switch
        {
            "popular" => query.OrderByDescending(t => t.SalesCount),
            "rating" => query.OrderByDescending(t => t.AverageRating),
            "price-asc" => query.OrderBy(t => t.SalePrice ?? t.Price),
            "price-desc" => query.OrderByDescending(t => t.SalePrice ?? t.Price),
            _ => query.OrderByDescending(t => t.CreatedAt) // newest default
        };

        var total = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }
    // TemplateRepository.cs thêm:
    public async Task<bool> CategoryExistsAsync(int categoryId) =>
        await _context.Categories.AnyAsync(c => c.Id == categoryId);

    public async Task<bool> AllTagsExistAsync(List<int> tagIds) =>
        await _context.Tags.CountAsync(t => tagIds.Contains(t.Id)) == tagIds.Count;
    public async Task DeleteChildrenAsync(Guid templateId)
    {
        await _context.TemplateImages
            .Where(i => i.TemplateId == templateId)
            .ExecuteDeleteAsync();

        await _context.TemplateTags
            .Where(tt => tt.TemplateId == templateId)
            .ExecuteDeleteAsync();

        await _context.TemplateFeatures
            .Where(f => f.TemplateId == templateId)
            .ExecuteDeleteAsync();

        await _context.TemplateVersions
            .Where(v => v.TemplateId == templateId)
            .ExecuteDeleteAsync();

        await _context.Wishlists
            .Where(w => w.TemplateId == templateId)
            .ExecuteDeleteAsync();

        await _context.Reviews
            .Where(r => r.TemplateId == templateId)
            .ExecuteDeleteAsync();
        await _context.MediaFiles
    .Where(m => m.TemplateId == templateId)
    .ExecuteDeleteAsync();

    }
}