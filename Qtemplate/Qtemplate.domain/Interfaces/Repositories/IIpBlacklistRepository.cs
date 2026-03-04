using Qtemplate.Domain.Entities;

namespace Qtemplate.Domain.Interfaces.Repositories;

public interface IIpBlacklistRepository
{
    Task<bool> IsBlockedAsync(string ipAddress);
    Task<(List<IpBlacklist> Items, int Total)> GetPagedAsync(int page, int pageSize);
    Task<IpBlacklist?> GetByIdAsync(int id);
    Task<IpBlacklist?> GetByIpAsync(string ipAddress);
    Task AddAsync(IpBlacklist entry);
    Task UpdateAsync(IpBlacklist entry);
    Task DeleteAsync(IpBlacklist entry);
}