using Qtemplate.Domain.Entities;

namespace Qtemplate.Domain.Interfaces.Repositories;

public interface IUserRepository
{
    
    Task<User?> GetByIdAsync(Guid id);
    Task<User?> GetActiveByIdAsync(Guid id);
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByResetTokenAsync(string token);           // ← thêm
    Task<User?> GetByEmailVerifyTokenAsync(string token);     // ← thêm
    Task<bool> EmailExistsAsync(string email);
    Task AddAsync(User user);
    Task UpdateAsync(User user);
    Task<(List<User> Items, int Total)> GetPagedAsync(
       string? search, string? role, bool? isActive, int page, int pageSize);
    Task<List<Guid>> GetAllActiveUserIdsAsync();
}