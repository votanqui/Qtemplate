using Qtemplate.Domain.Entities;

namespace Qtemplate.Domain.Interfaces.Repositories;

public interface IPostRepository
{
    // Public
    Task<(List<Post> Items, int Total)> GetPublishedAsync(int page, int pageSize, string? search, bool? isFeatured);
    Task<Post?> GetBySlugAsync(string slug);
    Task IncrementViewCountAsync(int id);

    // Admin
    Task<(List<Post> Items, int Total)> GetAdminListAsync(int page, int pageSize, string? search, string? status);
    Task<Post?> GetByIdAsync(int id);
    Task AddAsync(Post post);
    Task UpdateAsync(Post post);
    Task DeleteAsync(Post post);
    Task<bool> SlugExistsAsync(string slug, int? excludeId = null);
}