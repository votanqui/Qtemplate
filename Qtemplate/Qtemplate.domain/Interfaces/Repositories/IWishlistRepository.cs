using Qtemplate.Domain.Entities;

namespace Qtemplate.Domain.Interfaces.Repositories;

public interface IWishlistRepository
{
    Task<(List<Wishlist> Items, int Total)> GetPagedByUserIdAsync(Guid userId, int page, int pageSize);
    Task<Wishlist?> GetAsync(Guid userId, Guid templateId);
    Task AddAsync(Wishlist wishlist);
    Task RemoveAsync(Wishlist wishlist);
    Task<bool> ExistsAsync(Guid userId, Guid templateId);
    Task<List<Guid>> GetTemplateIdsByUserAsync(Guid userId);
    Task<(List<Wishlist> Items, int Total)> GetPagedAdminAsync(Guid? userId, Guid? templateId, int page, int pageSize);
    Task<List<(Guid TemplateId, string TemplateName, int Count)>> GetTopWishlistedAsync(int top);
}