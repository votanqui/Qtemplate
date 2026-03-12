namespace Qtemplate.Application.DTOs.AuditLog;

public class AuditLogDto
{
    public long Id { get; set; }
    public string? UserId { get; set; }
    public string? UserEmail { get; set; }
    public string Action { get; set; } = string.Empty;
    public string EntityName { get; set; } = string.Empty;
    public string? EntityId { get; set; }
    public string? OldValues { get; set; }   // raw JSON string
    public string? NewValues { get; set; }   // raw JSON string
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public DateTime CreatedAt { get; set; }
}