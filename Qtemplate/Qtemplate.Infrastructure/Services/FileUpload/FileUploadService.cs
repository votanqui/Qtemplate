using Microsoft.Extensions.Hosting;
using Qtemplate.Application.Services.Interfaces;
using System.IO.Compression;

namespace Qtemplate.Infrastructure.Services.FileUpload;

public class FileUploadService : IFileUploadService
{
    private readonly string _webRootPath;
    private readonly string _privateStoragePath;

    private static readonly Dictionary<string, byte[]> _allowedSignatures = new()
    {
        { ".jpg",  new byte[] { 0xFF, 0xD8, 0xFF } },
        { ".jpeg", new byte[] { 0xFF, 0xD8, 0xFF } },
        { ".png",  new byte[] { 0x89, 0x50, 0x4E, 0x47 } },
        { ".webp", new byte[] { 0x52, 0x49, 0x46, 0x46 } },
    };

    private const long MaxImageSizeBytes = 10 * 1024 * 1024;  // 5MB
    private const long MaxZipSizeBytes = 50 * 1024 * 1024;  // 50MB

    public FileUploadService(IHostEnvironment env)
    {
        _webRootPath = Path.Combine(env.ContentRootPath, "wwwroot");
        _privateStoragePath = Path.Combine(env.ContentRootPath, "private-storage");
    }

    // ── Shared image save ─────────────────────────────────────────────────────
    private async Task<string> SaveImageAsync(Stream fileStream, string originalFileName, long fileSize, string folder)
    {
        if (fileSize <= 0 || fileSize > MaxImageSizeBytes)
            throw new InvalidOperationException("File phải có dung lượng từ 1 byte đến 5MB");

        var ext = Path.GetExtension(originalFileName).ToLower().Trim();
        if (!_allowedSignatures.ContainsKey(ext))
            throw new InvalidOperationException("Chỉ chấp nhận file ảnh định dạng JPG, PNG, WEBP");

        var magicBytes = new byte[12];
        var bytesRead = await fileStream.ReadAsync(magicBytes, 0, magicBytes.Length);
        fileStream.Position = 0;

        var expected = _allowedSignatures[ext];
        if (bytesRead < expected.Length || !magicBytes.Take(expected.Length).SequenceEqual(expected))
            throw new InvalidOperationException("Nội dung file không hợp lệ, có thể bị giả mạo định dạng");

        if (ext == ".webp")
        {
            var webpMarker = new byte[] { 0x57, 0x45, 0x42, 0x50 };
            if (bytesRead < 12 || !magicBytes.Skip(8).Take(4).SequenceEqual(webpMarker))
                throw new InvalidOperationException("Nội dung file WEBP không hợp lệ");
        }

        var safeFileName = $"{Guid.NewGuid():N}{ext}";
        var uploadFolder = Path.Combine(_webRootPath, folder);
        Directory.CreateDirectory(uploadFolder);

        var fullPath = Path.Combine(uploadFolder, safeFileName);
        await using var fs = new FileStream(fullPath, FileMode.Create, FileAccess.Write);
        await fileStream.CopyToAsync(fs);

        return $"/{folder}/{safeFileName}";
    }

    public Task<string> SaveAvatarAsync(Stream s, string name, long size)
        => SaveImageAsync(s, name, size, "avatarUser");

    public Task<string> SaveThumbnailAsync(Stream s, string name, long size)
        => SaveImageAsync(s, name, size, "thumbnails");

    public Task<string> SaveTemplateImageAsync(Stream s, string name, long size)
        => SaveImageAsync(s, name, size, "template-images");

    public Task<string> SaveBannerImageAsync(Stream s, string name, long size)
        => SaveImageAsync(s, name, size, "banners");

    // ── Preview ZIP → giải nén + lưu ZIP gốc ────────────────────────────────
    public async Task<string> SavePreviewZipAsync(
     Stream stream, string fileName, long fileSize, Guid templateId)
    {
        // Validate
        if (!fileName.EndsWith(".zip", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Chỉ hỗ trợ file .zip");

        var previewFolder = Path.Combine(
            Directory.GetCurrentDirectory(),
            "private-storage", "previews", templateId.ToString());

        // Xóa folder cũ nếu có
        if (Directory.Exists(previewFolder))
            Directory.Delete(previewFolder, recursive: true);

        Directory.CreateDirectory(previewFolder);

        // Giải nén ZIP vào previewFolder
        using var zip = new System.IO.Compression.ZipArchive(stream, System.IO.Compression.ZipArchiveMode.Read);
        zip.ExtractToDirectory(previewFolder, overwriteFiles: true);

        // Trả về relative path
        return $"previews/{templateId}";
    }

    // ── Delete helpers ────────────────────────────────────────────────────────
    public void DeleteAvatar(string? relativeUrl) => DeleteFileInWebRoot("avatarUser", relativeUrl);
    public void DeleteThumbnail(string? relativeUrl) => DeleteFileInWebRoot("thumbnails", relativeUrl);
    public void DeleteBannerImage(string? relativeUrl) => DeleteFileInWebRoot("banners", relativeUrl);
    public void DeleteTemplateImage(string? relativeUrl) => DeleteFileInWebRoot("template-images", relativeUrl);

    public void DeletePreview(Guid templateId)
    {
        var folder = GetPreviewPhysicalPath(templateId);
        if (Directory.Exists(folder))
            Directory.Delete(folder, recursive: true);
    }

    public void DeleteDownloadFile(Guid templateId)
    {
        var path = Path.Combine(_privateStoragePath, "downloads", $"{templateId:N}.zip");
        if (File.Exists(path)) File.Delete(path);
    }

    public string GetPreviewPhysicalPath(Guid templateId) =>
        Path.Combine(_privateStoragePath, "previews", templateId.ToString());

    private void DeleteFileInWebRoot(string folder, string? relativeUrl)
    {
        if (string.IsNullOrEmpty(relativeUrl)) return;
        var fileName = Path.GetFileName(relativeUrl);
        if (string.IsNullOrEmpty(fileName)) return;
        var fullPath = Path.Combine(_webRootPath, folder, fileName);
        if (File.Exists(fullPath)) File.Delete(fullPath);
    }
    public async Task<string> SaveVersionZipAsync(
    Stream zipStream, string originalFileName, long fileSize, Guid templateId, string version)
    {
        if (fileSize <= 0 || fileSize > MaxZipSizeBytes)
            throw new InvalidOperationException("File ZIP phải từ 1 byte đến 50MB");

        if (Path.GetExtension(originalFileName).ToLower() != ".zip")
            throw new InvalidOperationException("Chỉ chấp nhận file .zip");

        using var ms = new MemoryStream();
        await zipStream.CopyToAsync(ms);
        ms.Position = 0;

        var magic = new byte[4];
        await ms.ReadAsync(magic, 0, 4);
        if (magic[0] != 0x50 || magic[1] != 0x4B)
            throw new InvalidOperationException("File không phải ZIP hợp lệ");

        var dir = Path.Combine(_privateStoragePath, "versions", templateId.ToString());
        Directory.CreateDirectory(dir);

        var safeVersion = version.Replace(".", "_");
        var fileName = $"{safeVersion}.zip";
        var fullPath = Path.Combine(dir, fileName);

        ms.Position = 0;
        await using var fs = new FileStream(fullPath, FileMode.Create);
        await ms.CopyToAsync(fs);

        return $"/versions/{templateId}/{fileName}";
    }

    public void DeleteVersionZip(Guid templateId, string version)
    {
        var safeVersion = version.Replace(".", "_");
        var path = Path.Combine(_privateStoragePath, "versions", templateId.ToString(), $"{safeVersion}.zip");
        if (File.Exists(path)) File.Delete(path);
    }
    public string GetDownloadPhysicalPath(Guid templateId, string? downloadPath = null)
    {
        if (!string.IsNullOrEmpty(downloadPath))
        {
            // "/downloads/02ab6a6a...zip" → private-storage/downloads/02ab6a6a...zip
            // "/versions/{id}/1_0_0.zip" → private-storage/versions/{id}/1_0_0.zip
            var relativePath = downloadPath.TrimStart('/');
            return Path.Combine(_privateStoragePath, relativePath);
        }

        // Fallback nếu không có downloadPath
        return Path.Combine(_privateStoragePath, "downloads", $"{templateId:N}.zip");
    }
    // Lưu file vào downloads/{templateId}.zip
    public async Task<string> SaveDownloadZipAsync(
        Stream zipStream, string originalFileName, long fileSize, Guid templateId)
    {
        if (fileSize <= 0 || fileSize > MaxZipSizeBytes)
            throw new InvalidOperationException("File ZIP phải từ 1 byte đến 50MB");

        if (Path.GetExtension(originalFileName).ToLower() != ".zip")
            throw new InvalidOperationException("Chỉ chấp nhận file .zip");

        // Validate magic bytes
        using var ms = new MemoryStream();
        await zipStream.CopyToAsync(ms);
        ms.Position = 0;

        var magic = new byte[4];
        await ms.ReadAsync(magic, 0, 4);
        if (magic[0] != 0x50 || magic[1] != 0x4B)
            throw new InvalidOperationException("File không phải ZIP hợp lệ");

        var dir = Path.Combine(_privateStoragePath, "downloads");
        Directory.CreateDirectory(dir);

        var fileName = $"{templateId:N}.zip";
        var fullPath = Path.Combine(dir, fileName);

        // Xóa file cũ nếu có
        if (File.Exists(fullPath))
            File.Delete(fullPath);

        ms.Position = 0;
        await using var fs = new FileStream(fullPath, FileMode.Create);
        await ms.CopyToAsync(fs);

        return $"/downloads/{fileName}";   // không có / đầu
    }

    public void DeleteDownloadByUrl(string? url)
    {
        if (string.IsNullOrEmpty(url)) return;
        var filePath = Path.Combine(
            _privateStoragePath,
            url.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
        if (File.Exists(filePath)) File.Delete(filePath);
    }
}