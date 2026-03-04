using Microsoft.AspNetCore.Http;

namespace Qtemplate.Application.DTOs.Media;

public class MediaFileDto
{
    public int Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string OriginalName { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string? MimeType { get; set; }
    public long FileSize { get; set; }
    public string FileSizeText { get; set; } = string.Empty;
    public string StorageType { get; set; } = string.Empty;
    public string? ExternalId { get; set; }
    public Guid? TemplateId { get; set; }
    public string? TemplateName { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class LinkMediaDto
{
    public string Url { get; set; } = string.Empty;
    public string OriginalName { get; set; } = string.Empty;
    public string StorageType { get; set; } = "GoogleDrive";
    public string? ExternalId { get; set; }
    public Guid? TemplateId { get; set; }
}

public class SetDownloadFileDto
{
    public int MediaFileId { get; set; }
}
public class UploadMediaRequest
{
    public IFormFile File { get; set; } = null!;
    public Guid? TemplateId { get; set; }
}