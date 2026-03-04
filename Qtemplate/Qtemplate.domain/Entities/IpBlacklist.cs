namespace Qtemplate.Domain.Entities;

public class IpBlacklist
{
    public int Id { get; set; }
    public string IpAddress { get; set; } = string.Empty;
    public string? Reason { get; set; }
    public string Type { get; set; } = "Manual";            // Manual / Auto
    public string? BlockedBy { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime? ExpiredAt { get; set; }                // Null = block vĩnh viễn
    public DateTime BlockedAt { get; set; } = DateTime.UtcNow;
}