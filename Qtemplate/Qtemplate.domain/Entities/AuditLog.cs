namespace Qtemplate.Domain.Entities;

public class AuditLog
{
    public long Id { get; set; }
    public string? UserId { get; set; }                     // Admin thực hiện
    public string? UserEmail { get; set; }
    public string Action { get; set; } = string.Empty;      // Create / Update / Delete
    public string EntityName { get; set; } = string.Empty;  // "Template", "Order"...
    public string? EntityId { get; set; }                   // Id của record bị tác động
    public string? OldValues { get; set; }                  // JSON giá trị trước khi sửa
    public string? NewValues { get; set; }                  // JSON giá trị sau khi sửa
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}