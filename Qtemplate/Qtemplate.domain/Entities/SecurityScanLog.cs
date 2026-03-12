namespace Qtemplate.Domain.Entities;
public class SecurityScanLog
{
    public long Id { get; set; }

    public string Violation { get; set; } = string.Empty;

    public string? IpAddress { get; set; }

    public Guid? UserId { get; set; }

    public string Reason { get; set; } = string.Empty;

    public string Action { get; set; } = string.Empty;
    public bool IsAdminOverride { get; set; } = false;

    public string? OverrideByEmail { get; set; }
    public string? OverrideNote { get; set; }
    public DateTime? OverrideAt { get; set; }

    public DateTime ScannedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public User? User { get; set; }
}