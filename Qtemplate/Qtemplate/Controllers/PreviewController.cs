// Controllers/PreviewController.cs
using Microsoft.AspNetCore.Mvc;
using Qtemplate.Application.Services.Interfaces;
using Qtemplate.Domain.Interfaces.Repositories;

namespace Qtemplate.Controllers;

[ApiController]
[Route("api/preview")]
public class PreviewController : ControllerBase
{
    private readonly ITemplateRepository _templateRepo;
    private readonly IFileUploadService _fileUploadService;

    private static readonly Dictionary<string, string> _mimeTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        { ".html", "text/html; charset=utf-8" },
        { ".php",  "text/html; charset=utf-8" }, // ← thêm
        { ".htm",  "text/html; charset=utf-8" },
        { ".css",  "text/css" },
        { ".js",   "application/javascript" },
        { ".png",  "image/png" },
        { ".jpg",  "image/jpeg" },
        { ".jpeg", "image/jpeg" },
        { ".webp", "image/webp" },
        { ".svg",  "image/svg+xml" },
        { ".ico",  "image/x-icon" },
        { ".woff", "font/woff" },
        { ".woff2","font/woff2" },
        { ".ttf",  "font/ttf" },
        { ".json", "application/json" },
        { ".xml",  "application/xml" },
        { ".txt",  "text/plain" },
        { ".mp4",  "video/mp4" },
        { ".gif",  "image/gif" },
    };

    public PreviewController(ITemplateRepository templateRepo, IFileUploadService fileUploadService)
    {
        _templateRepo = templateRepo;
        _fileUploadService = fileUploadService;
    }

    [HttpGet("{templateId:guid}")]
    public Task<IActionResult> ServeIndex(Guid templateId) => Serve(templateId, "index.html");

    [HttpGet("{templateId:guid}/{**filePath}")]
    public async Task<IActionResult> Serve(Guid templateId, string filePath)
    {
        var template = await _templateRepo.GetByIdAsync(templateId);
        if (template is null || string.IsNullOrEmpty(template.PreviewFolder))
            return NotFound(new { message = "Template không tồn tại hoặc chưa có preview" });

        // Chặn path traversal
        if (filePath.Contains(".."))
            return BadRequest();

        var basePath = _fileUploadService.GetPreviewPhysicalPath(templateId);

        if (!Directory.Exists(basePath))
            return NotFound(new { message = "Thư mục preview không tồn tại, vui lòng upload lại" });

        var fullPath = Path.GetFullPath(Path.Combine(basePath, filePath));

        if (!fullPath.StartsWith(basePath + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            return BadRequest();

        if (!System.IO.File.Exists(fullPath))
        {
            // Tự động fallback về index.html nếu không tìm thấy file
            var indexPath = Path.Combine(basePath, "index.html");
            if (System.IO.File.Exists(indexPath))
                fullPath = indexPath;
            else
                return NotFound(new { message = $"File '{filePath}' không tồn tại trong preview" });
        }

        var ext = Path.GetExtension(fullPath);
        var mimeType = _mimeTypes.GetValueOrDefault(ext, "application/octet-stream");

        Response.Headers["Content-Disposition"] = "inline";
        Response.Headers["Cache-Control"] = "no-store";

        var bytes = await System.IO.File.ReadAllBytesAsync(fullPath);
        return File(bytes, mimeType);
    }
}