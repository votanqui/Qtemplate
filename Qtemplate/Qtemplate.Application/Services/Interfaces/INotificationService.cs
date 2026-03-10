namespace Qtemplate.Application.Services.Interfaces;

public interface INotificationService
{
    // Gửi đến 1 user cụ thể
    Task SendToUserAsync(Guid userId, string title, string message,
                         string type = "Info", string? redirectUrl = null);

    // Broadcast đến tất cả user đang online
    Task BroadcastAsync(string title, string message,
                        string type = "Info", string? redirectUrl = null);

    // Gửi đến nhiều user (theo danh sách userId)
    Task SendToUsersAsync(IEnumerable<Guid> userIds, string title, string message,
                          string type = "Info", string? redirectUrl = null);
}