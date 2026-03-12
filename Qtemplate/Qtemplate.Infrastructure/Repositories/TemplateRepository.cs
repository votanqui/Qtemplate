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

    // ── GetPublicListAsync — tìm kiếm nâng cao ───────────────────────────────
    public async Task<(List<Template> Items, int Total)> GetPublicListAsync(
        string? search,
        string? categorySlug,
        string? tagSlug,
        bool? isFree,
        decimal? minPrice,
        decimal? maxPrice,
        string sortBy,
        int page,
        int pageSize,
        bool? onSale = null,
        bool? isFeatured = null,
        bool? isNew = null,
        string? techStack = null)
    {
        var now = DateTime.UtcNow;

        var query = _context.Templates
            .Include(t => t.Category)
            .Include(t => t.TemplateTags).ThenInclude(tt => tt.Tag)
            .Include(t => t.Features)
            .Where(t => t.Status == "Published")
            .AsQueryable();

        // ── Search nâng cao ──────────────────────────────────────────────────
        // Tìm trong: Name, ShortDescription, TechStack, CompatibleWith,
        //            Tag names, Feature names, Category name
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            query = query.Where(t =>
                t.Name.Contains(s) ||
                (t.ShortDescription != null && t.ShortDescription.Contains(s)) ||
                (t.Description != null && t.Description.Contains(s)) ||
                (t.TechStack != null && t.TechStack.Contains(s)) ||
                (t.CompatibleWith != null && t.CompatibleWith.Contains(s)) ||
                t.Category.Name.Contains(s) ||
                t.TemplateTags.Any(tt => tt.Tag.Name.Contains(s)) ||
                t.Features.Any(f => f.Feature.Contains(s)));
        }

        // ── Filters ──────────────────────────────────────────────────────────
        if (!string.IsNullOrEmpty(categorySlug))
            query = query.Where(t => t.Category.Slug == categorySlug);

        if (!string.IsNullOrEmpty(tagSlug))
            query = query.Where(t => t.TemplateTags.Any(tt => tt.Tag.Slug == tagSlug));

        if (isFree.HasValue)
            query = query.Where(t => t.IsFree == isFree.Value);

        if (isFeatured.HasValue)
            query = query.Where(t => t.IsFeatured == isFeatured.Value);

        if (isNew.HasValue)
            query = query.Where(t => t.IsNew == isNew.Value);

        if (!string.IsNullOrWhiteSpace(techStack))
            query = query.Where(t => t.TechStack != null && t.TechStack.Contains(techStack));

        // onSale: chỉ lấy template đang sale hợp lệ
        // (SalePrice != null, SaleStartAt <= now hoặc null, SaleEndAt > now hoặc null)
        if (onSale == true)
            query = query.Where(t =>
                t.SalePrice != null &&
                !t.IsFree &&
                (t.SaleStartAt == null || t.SaleStartAt <= now) &&
                (t.SaleEndAt == null || t.SaleEndAt > now));

        // Price filter: dùng effective price (SalePrice nếu đang sale hợp lệ, ngược lại Price)
        if (minPrice.HasValue)
            query = query.Where(t =>
                (t.SalePrice != null && (t.SaleStartAt == null || t.SaleStartAt <= now) && (t.SaleEndAt == null || t.SaleEndAt > now)
                    ? t.SalePrice.Value
                    : t.Price) >= minPrice.Value);

        if (maxPrice.HasValue)
            query = query.Where(t =>
                (t.SalePrice != null && (t.SaleStartAt == null || t.SaleStartAt <= now) && (t.SaleEndAt == null || t.SaleEndAt > now)
                    ? t.SalePrice.Value
                    : t.Price) <= maxPrice.Value);

        // ── Sort ─────────────────────────────────────────────────────────────
        query = sortBy switch
        {
            "popular" => query.OrderByDescending(t => t.SalesCount),
            "rating" => query.OrderByDescending(t => t.AverageRating),
            "price-asc" => query.OrderBy(t => t.SalePrice != null &&
                                (t.SaleStartAt == null || t.SaleStartAt <= now) &&
                                (t.SaleEndAt == null || t.SaleEndAt > now)
                                    ? t.SalePrice.Value : t.Price),
            "price-desc" => query.OrderByDescending(t => t.SalePrice != null &&
                                (t.SaleStartAt == null || t.SaleStartAt <= now) &&
                                (t.SaleEndAt == null || t.SaleEndAt > now)
                                    ? t.SalePrice.Value : t.Price),
            "discount" => query.OrderByDescending(t =>
                                t.SalePrice != null ? (t.Price - t.SalePrice.Value) / t.Price : 0),
            _ => query.OrderByDescending(t => t.CreatedAt)  // newest default
        };

        var total = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }

    // ── GetOnSaleListAsync — dành cho trang Săn Sale ─────────────────────────
    public async Task<(List<Template> Items, int Total)> GetOnSaleListAsync(
        string? search,
        string? categorySlug,
        int page,
        int pageSize)
    {
        var now = DateTime.UtcNow;

        var query = _context.Templates
            .Include(t => t.Category)
            .Include(t => t.TemplateTags).ThenInclude(tt => tt.Tag)
            .Include(t => t.Features)
            .Where(t =>
                t.Status == "Published" &&
                !t.IsFree &&
                t.SalePrice != null &&
                (t.SaleStartAt == null || t.SaleStartAt <= now) &&
                (t.SaleEndAt == null || t.SaleEndAt > now))
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            query = query.Where(t =>
                t.Name.Contains(s) ||
                (t.ShortDescription != null && t.ShortDescription.Contains(s)) ||
                t.Category.Name.Contains(s) ||
                t.TemplateTags.Any(tt => tt.Tag.Name.Contains(s)));
        }

        if (!string.IsNullOrEmpty(categorySlug))
            query = query.Where(t => t.Category.Slug == categorySlug);

        // Sắp xếp theo % giảm nhiều nhất
        query = query.OrderByDescending(t => (t.Price - t.SalePrice!.Value) / t.Price);

        var total = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }

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
    public async Task ReplaceTagsAndFeaturesAsync(
        Guid templateId,
        List<int> tagIds,
        List<(string Feature, int SortOrder)> features)
    {
        // ExecuteDelete đi thẳng xuống DB nhưng EF context vẫn còn track
        // các entity cũ từ GetByIdWithDetailsAsync → phải Clear trước khi add mới
        // để tránh "another instance with same key is already being tracked"
        await _context.TemplateTags
            .Where(t => t.TemplateId == templateId)
            .ExecuteDeleteAsync();

        await _context.TemplateFeatures
            .Where(f => f.TemplateId == templateId)
            .ExecuteDeleteAsync();

        // Clear toàn bộ change tracker — safe vì UpdateAsync đã SaveChanges xong rồi
        _context.ChangeTracker.Clear();

        if (tagIds.Count > 0)
        {
            var newTags = tagIds.Select(tagId => new TemplateTag
            {
                TemplateId = templateId,
                TagId = tagId,
            }).ToList();
            await _context.TemplateTags.AddRangeAsync(newTags);
        }

        if (features.Count > 0)
        {
            var newFeatures = features.Select(f => new TemplateFeature
            {
                TemplateId = templateId,
                Feature = f.Feature,
                SortOrder = f.SortOrder,
            }).ToList();
            await _context.TemplateFeatures.AddRangeAsync(newFeatures);
        }

        await _context.SaveChangesAsync();
    }
    public async Task<int> BulkSetSaleAsync(
       List<Guid> templateIds, decimal? salePrice, DateTime? saleStartAt, DateTime? saleEndAt)
    {
        var templates = await _context.Templates
            .Where(t => templateIds.Contains(t.Id) && !t.IsFree)
            .ToListAsync();

        if (salePrice.HasValue)
            // Chỉ update những template có giá gốc > salePrice
            templates = templates.Where(t => salePrice < t.Price).ToList();

        if (templates.Count == 0) return 0;

        var now = DateTime.UtcNow;
        foreach (var t in templates)
        {
            t.SalePrice = salePrice;
            t.SaleStartAt = salePrice.HasValue ? saleStartAt : null;
            t.SaleEndAt = salePrice.HasValue ? saleEndAt : null;
            t.UpdatedAt = now;
        }

        await _context.SaveChangesAsync();
        return templates.Count;
    }
}