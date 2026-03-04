namespace Qtemplate.Domain.Entities;

public class TemplateVersion
{
    public int Id { get; set; }
    public Guid TemplateId { get; set; }
    public string Version { get; set; } = string.Empty;     // "1.0.0", "1.1.0"
    public string? ChangeLog { get; set; }                  // Mô tả thay đổi
    public string? DownloadPath { get; set; }               // File zip của version này
    public bool IsLatest { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Template Template { get; set; } = null!;
}