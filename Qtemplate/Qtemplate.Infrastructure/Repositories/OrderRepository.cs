using Microsoft.EntityFrameworkCore;
using Qtemplate.Domain.Entities;
using Qtemplate.Domain.Interfaces.Repositories;
using Qtemplate.Infrastructure.Data;

namespace Qtemplate.Infrastructure.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly AppDbContext _context;
    public OrderRepository(AppDbContext context) => _context = context;

    public async Task<Order?> GetByIdAsync(Guid id) =>
        await _context.Orders.FindAsync(id);

    public async Task<Order?> GetByIdWithDetailsAsync(Guid id) =>
        await _context.Orders
            .Include(o => o.User)
            .Include(o => o.Items).ThenInclude(i => i.Template)
            .Include(o => o.Payment)
            .FirstOrDefaultAsync(o => o.Id == id);

    public async Task<Order?> GetByOrderCodeAsync(string orderCode) =>
        await _context.Orders
            .Include(o => o.Items).ThenInclude(i => i.Template)
            .Include(o => o.Payment)
            .FirstOrDefaultAsync(o => o.OrderCode == orderCode);

    public async Task<(List<Order> items, int total)> GetPagedByUserIdAsync(
      Guid userId, int page, int pageSize, string? status = null)
    {
        var query = _context.Orders
            .Include(o => o.Items).ThenInclude(i => i.Template)
            .Include(o => o.Payment)
            .Where(o => o.UserId == userId);

        if (!string.IsNullOrEmpty(status))
            query = query.Where(o => o.Status.ToString() == status); // ← thêm dòng này

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }
    public async Task<(List<Order> Items, int Total)> GetAdminListAsync(
        string? status, Guid? userId, string? search, int page, int pageSize)
    {
        var query = _context.Orders
            .Include(o => o.User)
            .Include(o => o.Items)
            .Include(o => o.Payment)
            .AsQueryable();

        if (!string.IsNullOrEmpty(status))
            query = query.Where(o => o.Status == status);

        if (userId.HasValue)
            query = query.Where(o => o.UserId == userId.Value);

        if (!string.IsNullOrEmpty(search))
            query = query.Where(o =>
                o.OrderCode.Contains(search) ||
                o.User.Email.Contains(search));

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }

    public async Task AddAsync(Order order)
    {
        await _context.Orders.AddAsync(order);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Order order)
    {
        _context.Orders.Update(order);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> HasPurchasedAsync(Guid userId, Guid templateId) =>
        await _context.OrderItems
            .AnyAsync(oi =>
                oi.Order.UserId == userId &&
                oi.TemplateId == templateId &&
                (oi.Order.Status == "Paid" || oi.Order.Status == "Completed"));
    public async Task<Order?> GetPaidOrderByUserAndTemplateAsync(Guid userId, Guid templateId) =>
        await _context.Orders
            .Include(o => o.Items)   // ← cần include để .Any() hoạt động đúng
            .Where(o => o.UserId == userId
                     && (o.Status == "Paid" || o.Status == "Completed")
                     && o.Items.Any(i => i.TemplateId == templateId))
            .OrderByDescending(o => o.CreatedAt)
            .FirstOrDefaultAsync();
    public async Task<List<Order>> GetByDateRangeAsync(DateTime from, DateTime to) =>
    await _context.Orders
        .Include(o => o.User)
        .Where(o => o.CreatedAt >= from && o.CreatedAt <= to)
        .ToListAsync();

    public async Task<List<Order>> GetByDateRangeWithItemsAsync(DateTime from, DateTime to) =>
        await _context.Orders
            .Include(o => o.User)
            .Include(o => o.Items)
                .ThenInclude(i => i.Template)
            .Where(o => o.CreatedAt >= from && o.CreatedAt <= to)
            .ToListAsync();
    public async Task<(int TotalOrders, decimal TotalSpent)> GetUserStatsAsync(Guid userId)
    {
        var stats = await _context.Orders
            .Where(o => o.UserId == userId
                     && (o.Status == "Paid" || o.Status == "Completed"))
            .GroupBy(_ => 1)
            .Select(g => new
            {
                TotalOrders = g.Count(),
                TotalSpent = g.Sum(o => o.FinalAmount)
            })
            .FirstOrDefaultAsync();

        return stats is null ? (0, 0m) : (stats.TotalOrders, stats.TotalSpent);
    }
    public async Task<List<(Guid UserId, int Count)>> GetCancelSpamUsersAsync(
    DateTime from, int threshold)
    {
        var rows = await _context.Orders
            .Where(o => o.CancelledAt >= from && o.Status == "Cancelled")
            .GroupBy(o => o.UserId)
            .Where(g => g.Count() >= threshold)
            .Select(g => new { UserId = g.Key, Count = g.Count() })
            .ToListAsync();

        return rows.Select(x => (x.UserId, x.Count)).ToList();
    }
}