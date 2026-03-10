using Microsoft.EntityFrameworkCore;
using Qtemplate.Domain.Entities;
using Qtemplate.Domain.Interfaces.Repositories;
using Qtemplate.Infrastructure.Data;

namespace Qtemplate.Infrastructure.Repositories;

public class NotificationRepository : INotificationRepository
{
    private readonly AppDbContext _context;
    public NotificationRepository(AppDbContext context) => _context = context;

    public async Task<(List<Notification> Items, int Total)> GetPagedByUserIdAsync(
        Guid userId, int page, int pageSize, bool? unreadOnly)
    {
        var query = _context.Notifications.Where(n => n.UserId == userId);

        if (unreadOnly == true)
            query = query.Where(n => !n.IsRead);

        query = query.OrderByDescending(n => n.CreatedAt);

        var total = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }

    public async Task<Notification?> GetByIdAsync(int id, Guid userId)
        => await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);

    public async Task<List<Notification>> GetUnreadByUserIdAsync(Guid userId)
        => await _context.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync();

    public async Task UpdateAsync(Notification notification)
    {
        _context.Notifications.Update(notification);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateRangeAsync(List<Notification> notifications)
    {
        _context.Notifications.UpdateRange(notifications);
        await _context.SaveChangesAsync();
    }
    public async Task AddAsync(Notification notification)
    {
        await _context.Notifications.AddAsync(notification);
        await _context.SaveChangesAsync();
    }

    public async Task AddRangeAsync(List<Notification> notifications)
    {
        await _context.Notifications.AddRangeAsync(notifications);
        await _context.SaveChangesAsync();
    }
}