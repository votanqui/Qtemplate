using Qtemplate.Domain.Entities;

namespace Qtemplate.Domain.Interfaces.Repositories;

public interface INotificationRepository
{
    Task<(List<Notification> Items, int Total)> GetPagedByUserIdAsync(Guid userId, int page, int pageSize, bool? unreadOnly);
    Task<Notification?> GetByIdAsync(int id, Guid userId);
    Task<List<Notification>> GetUnreadByUserIdAsync(Guid userId);
    Task UpdateAsync(Notification notification);
    Task UpdateRangeAsync(List<Notification> notifications);
    Task AddAsync(Notification notification);
    Task AddRangeAsync(List<Notification> notifications);
}