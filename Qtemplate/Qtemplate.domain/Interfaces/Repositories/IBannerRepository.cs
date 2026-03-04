using Qtemplate.Domain.Entities;

namespace Qtemplate.Domain.Interfaces.Repositories;

public interface IBannerRepository
{
    Task<List<Banner>> GetActiveByPositionAsync(string? position);
    Task<(List<Banner> Items, int Total)> GetAdminListAsync(int page, int pageSize);
    Task<Banner?> GetByIdAsync(int id);
    Task AddAsync(Banner banner);
    Task UpdateAsync(Banner banner);
    Task DeleteAsync(Banner banner);
}