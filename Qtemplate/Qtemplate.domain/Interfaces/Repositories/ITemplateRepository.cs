using Qtemplate.Domain.Entities;

namespace Qtemplate.Domain.Interfaces.Repositories;

public interface ITemplateRepository
{
    Task<Template?> GetByIdAsync(Guid id);
    Task UpdateAsync(Template template);
    Task<bool> SlugExistsAsync(string slug);
    Task<Template?> GetByIdWithDetailsAsync(Guid id);
    Task<(List<Template> Items, int Total)> GetAdminListAsync(string? search, string? status, int? categoryId, int page, int pageSize);
    Task AddAsync(Template template);
    Task DeleteAsync(Template template);
    Task<Template?> GetBySlugAsync(string slug);
    Task IncrementViewCountAsync(Guid id);
    Task<Template?> GetByIdFullAsync(Guid id);

    Task<(List<Template> Items, int Total)> GetPublicListAsync(
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
        string? techStack = null);
    Task<(List<Template> Items, int Total)> GetOnSaleListAsync(
        string? search,
        string? categorySlug,
        int page,
        int pageSize);

    Task<bool> CategoryExistsAsync(int categoryId);
    Task<bool> AllTagsExistAsync(List<int> tagIds);
    Task DeleteChildrenAsync(Guid templateId);
}