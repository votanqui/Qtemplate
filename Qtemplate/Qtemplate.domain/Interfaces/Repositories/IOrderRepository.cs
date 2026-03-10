using Qtemplate.Domain.Entities;

namespace Qtemplate.Domain.Interfaces.Repositories;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(Guid id);
    Task<Order?> GetByIdWithDetailsAsync(Guid id);
    Task<Order?> GetByOrderCodeAsync(string orderCode);
    Task<(List<Order> items, int total)> GetPagedByUserIdAsync(
     Guid userId, int page, int pageSize, string? status = null);
    Task<(List<Order> Items, int Total)> GetAdminListAsync(string? status, Guid? userId, string? search, int page, int pageSize);
    Task AddAsync(Order order);
    Task UpdateAsync(Order order);
    Task<bool> HasPurchasedAsync(Guid userId, Guid templateId);
    Task<Order?> GetPaidOrderByUserAndTemplateAsync(Guid userId, Guid templateId);
    Task<List<Order>> GetByDateRangeAsync(DateTime from, DateTime to);
    Task<List<Order>> GetByDateRangeWithItemsAsync(DateTime from, DateTime to);
    Task<(int TotalOrders, decimal TotalSpent)> GetUserStatsAsync(Guid userId);
}