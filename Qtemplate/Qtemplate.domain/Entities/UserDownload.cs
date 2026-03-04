namespace Qtemplate.Domain.Entities;

public class UserDownload
{
    public int Id { get; set; }
    public Guid UserId { get; set; }
    public Guid TemplateId { get; set; }
    public Guid OrderId { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? Device { get; set; }   // Desktop / Mobile / Tablet
    public string? Browser { get; set; }   // Chrome / Firefox / Safari
    public string? OS { get; set; }   // Windows / macOS / Android / iOS
    public int DownloadCount { get; set; } = 0;
    public DateTime? LastDownloadAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public User User { get; set; } = null!;
    public Template Template { get; set; } = null!;
}