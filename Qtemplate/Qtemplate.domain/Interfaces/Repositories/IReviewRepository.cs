using Qtemplate.Domain.Entities;

namespace Qtemplate.Domain.Interfaces.Repositories;

public interface IReviewRepository
{
    Task<Review?> GetByIdAsync(int id);
    Task<(List<Review> Items, int Total)> GetByTemplateSlugAsync(
        string slug, int page, int pageSize);
    Task<(List<Review> Items, int Total)> GetAdminListAsync(
        string? status, int page, int pageSize);
    Task<Review?> GetByUserAndTemplateAsync(Guid userId, Guid templateId);
    Task<List<Review>> GetByUserIdAsync(Guid userId);
    Task AddAsync(Review review);
    Task UpdateAsync(Review review);
    Task DeleteAsync(Review review);
    Task UpdateTemplateRatingAsync(Guid templateId);
    Task<List<Review>> GetPendingAiAsync(int limit = 10);
}