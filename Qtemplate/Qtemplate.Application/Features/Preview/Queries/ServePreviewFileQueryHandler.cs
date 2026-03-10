// Application/Features/Preview/Queries/ServePreviewFileQueryHandler.cs
using MediatR;
using Qtemplate.Application.Services.Interfaces;
using Qtemplate.Domain.Interfaces.Repositories;

namespace Qtemplate.Application.Features.Preview.Queries;

public class ServePreviewFileQueryHandler
    : IRequestHandler<ServePreviewFileQuery, PreviewFileResult>
{
    private readonly ITemplateRepository _templateRepo;
    private readonly IFileUploadService _fileUploadService;

    private static readonly Dictionary<string, string> MimeTypes =
        new(StringComparer.OrdinalIgnoreCase)
        {
            { ".html",  "text/html; charset=utf-8"  },
            { ".htm",   "text/html; charset=utf-8"  },
            { ".php",   "text/html; charset=utf-8"  },
            { ".css",   "text/css"                  },
            { ".js",    "application/javascript"    },
            { ".mjs",   "application/javascript"    },
            { ".json",  "application/json"          },
            { ".xml",   "application/xml"           },
            { ".svg",   "image/svg+xml"             },
            { ".png",   "image/png"                 },
            { ".jpg",   "image/jpeg"                },
            { ".jpeg",  "image/jpeg"                },
            { ".webp",  "image/webp"                },
            { ".gif",   "image/gif"                 },
            { ".ico",   "image/x-icon"              },
            { ".woff",  "font/woff"                 },
            { ".woff2", "font/woff2"                },
            { ".ttf",   "font/ttf"                  },
            { ".txt",   "text/plain"                },
            { ".mp4",   "video/mp4"                 },
        };

    public ServePreviewFileQueryHandler(
        ITemplateRepository templateRepo,
        IFileUploadService fileUploadService)
    {
        _templateRepo = templateRepo;
        _fileUploadService = fileUploadService;
    }

    public async Task<PreviewFileResult> Handle(
        ServePreviewFileQuery request,
        CancellationToken cancellationToken)
    {
        // 1. Validate template tồn tại và có preview
        var template = await _templateRepo.GetByIdAsync(request.TemplateId);
        if (template is null || string.IsNullOrEmpty(template.PreviewFolder))
            return PreviewFileResult.NotFound("Template không tồn tại hoặc chưa có preview");

        // 2. Chặn path traversal
        if (request.FilePath.Contains(".."))
            return PreviewFileResult.BadRequest("Đường dẫn không hợp lệ");

        // 3. Kiểm tra thư mục preview tồn tại
        var basePath = _fileUploadService.GetPreviewPhysicalPath(request.TemplateId);
        if (!Directory.Exists(basePath))
            return PreviewFileResult.NotFound("Thư mục preview không tồn tại, vui lòng upload lại");

        // 4. Resolve full path và kiểm tra nằm trong basePath (tránh traversal)
        var fullPath = Path.GetFullPath(Path.Combine(basePath, request.FilePath));
        if (!fullPath.StartsWith(basePath + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            return PreviewFileResult.BadRequest("Đường dẫn không hợp lệ");

        // 5. Fallback về index.html nếu file không tồn tại (hỗ trợ SPA React)
        if (!File.Exists(fullPath))
        {
            var indexPath = Path.Combine(basePath, "index.html");
            if (File.Exists(indexPath))
                fullPath = indexPath;
            else
                return PreviewFileResult.NotFound($"File '{request.FilePath}' không tồn tại trong preview");
        }

        var ext = Path.GetExtension(fullPath);
        var mimeType = MimeTypes.GetValueOrDefault(ext, "application/octet-stream");
        var bytes = await File.ReadAllBytesAsync(fullPath, cancellationToken);

        // 6. Với file HTML: inject <base> + rewrite absolute paths
        //
        //    Vấn đề với React/Vite build:
        //    Vite output thường dùng absolute path: src="/assets/index-abc.js"
        //    <base href="..."> KHÔNG ảnh hưởng absolute path (bắt đầu bằng /)
        //    → Cần rewrite "/assets/..." → "/api/preview/{id}/assets/..."
        //
        //    Với template HTML thuần (relative path: src="./assets/main.js"):
        //    <base href="..."> đã đủ để resolve
        //
        if (ext.Equals(".html", StringComparison.OrdinalIgnoreCase) ||
            ext.Equals(".htm", StringComparison.OrdinalIgnoreCase))
        {
            var html = System.Text.Encoding.UTF8.GetString(bytes);
            var baseUrl = $"/api/preview/{request.TemplateId}";
            var baseTag = $"<base href=\"{baseUrl}/\">";

            // Inject <base> sau <head> tag (xử lý mọi dạng: <head>, <head lang="vi">, ...)
            if (html.Contains("<head", StringComparison.OrdinalIgnoreCase))
            {
                html = System.Text.RegularExpressions.Regex.Replace(
                    html,
                    @"(<head[^>]*>)",
                    $"$1{baseTag}",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            }
            else
            {
                // Không có <head> → chèn đầu file (HTML fragment)
                html = baseTag + html;
            }

            // Rewrite absolute paths cho React/Vite build
            // Các pattern: src="/...", href="/...", url("/..."), từ="/..."
            // Chỉ rewrite path bắt đầu bằng "/" nhưng KHÔNG phải "//" (protocol-relative)
            // và KHÔNG phải đã có prefix /api/preview
            html = System.Text.RegularExpressions.Regex.Replace(
                html,
                // Bắt: src="/xxx", href="/xxx", content="/xxx" – không rewrite //cdn.com hay đã có baseUrl
                @"(src|href|content|action)=""(?!\/\/|https?:\/\/|#|data:|mailto:|tel:|" +
                    System.Text.RegularExpressions.Regex.Escape(baseUrl) +
                @")(\/[^""]*?)""",
                m => $"{m.Groups[1].Value}=\"{baseUrl}{m.Groups[2].Value}\"",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase
            );

            // Rewrite url('/...') và url("/...") trong CSS-in-HTML và inline style
            html = System.Text.RegularExpressions.Regex.Replace(
                html,
                @"url\(['""]?(?!\/\/|https?:\/\/|data:|" +
                    System.Text.RegularExpressions.Regex.Escape(baseUrl) +
                @")(\/[^'""\)]*?)['""]?\)",
                m => $"url(\"{baseUrl}{m.Groups[1].Value}\")",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase
            );

            bytes = System.Text.Encoding.UTF8.GetBytes(html);
        }

        // 7. Với file CSS: rewrite url('/...') absolute paths
        //    VD: background-image: url('/assets/bg.png') → url('/api/preview/{id}/assets/bg.png')
        if (ext.Equals(".css", StringComparison.OrdinalIgnoreCase))
        {
            var css = System.Text.Encoding.UTF8.GetString(bytes);
            var baseUrl = $"/api/preview/{request.TemplateId}";

            css = System.Text.RegularExpressions.Regex.Replace(
                css,
                @"url\(['""]?(?!\/\/|https?:\/\/|data:|" +
                    System.Text.RegularExpressions.Regex.Escape(baseUrl) +
                @")(\/[^'""\)]*?)['""]?\)",
                m => $"url(\"{baseUrl}{m.Groups[1].Value}\")",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase
            );

            bytes = System.Text.Encoding.UTF8.GetBytes(css);
        }

        return PreviewFileResult.Success(bytes, mimeType);
    }
}