namespace Qtemplate.Application.Services.Interfaces;

public interface IFileUploadService
{
    Task<string> SaveAvatarAsync(Stream fileStream, string originalFileName, long fileSize);
    Task<string> SaveThumbnailAsync(Stream fileStream, string originalFileName, long fileSize);
    Task<string> SavePreviewZipAsync(Stream stream, string fileName, long fileSize, Guid templateId);
    Task<string> SaveTemplateImageAsync(Stream fileStream, string originalFileName, long fileSize);
    Task<string> SaveVersionZipAsync(Stream zipStream, string originalFileName, long fileSize, Guid templateId, string version);
    void DeleteAvatar(string? relativeUrl);
    void DeleteThumbnail(string? relativeUrl);
    void DeleteTemplateImage(string? relativeUrl);
    void DeletePreview(Guid templateId);
    void DeleteDownloadFile(Guid templateId);
    void DeleteVersionZip(Guid templateId, string version);
    string GetPreviewPhysicalPath(Guid templateId);
    string GetDownloadPhysicalPath(Guid templateId, string? downloadPath = null); // ← thêm
    Task<string> SaveDownloadZipAsync(Stream zipStream, string originalFileName, long fileSize, Guid templateId);
    void DeleteDownloadByUrl(string? url);
}