namespace Qtemplate.Domain.Entities;

public class MediaFile
{
    public int Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string OriginalName { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;   // path hoặc external URL
    public string? MimeType { get; set; }
    public long FileSize { get; set; }
    public string StorageType { get; set; } = "Local";        // Local / GoogleDrive / S3 / R2
    public string? ExternalId { get; set; }
    public string? UploadedBy { get; set; }
    public Guid? TemplateId { get; set; }
    public bool IsDeleted { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public Template? Template { get; set; }
}