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

    private static readonly HashSet<string> StaticAssetExtensions =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ".js", ".mjs", ".css", ".png", ".jpg", ".jpeg", ".webp",
            ".gif", ".ico", ".svg", ".woff", ".woff2", ".ttf", ".json",
            ".xml", ".mp4", ".txt", ".map", ".gz", ".br",
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
        // 1. Validate template
        var template = await _templateRepo.GetByIdAsync(request.TemplateId);
        if (template is null || string.IsNullOrEmpty(template.PreviewFolder))
            return PreviewFileResult.NotFound("Template không tồn tại hoặc chưa có preview");

        // 2. Chặn path traversal
        if (request.FilePath.Contains(".."))
            return PreviewFileResult.BadRequest("Đường dẫn không hợp lệ");

        // 3. Kiểm tra thư mục tồn tại
        var basePath = _fileUploadService.GetPreviewPhysicalPath(request.TemplateId);
        if (!Directory.Exists(basePath))
            return PreviewFileResult.NotFound("Thư mục preview không tồn tại, vui lòng upload lại");

        // 4. Resolve & security check
        var fullPath = Path.GetFullPath(Path.Combine(basePath, request.FilePath));
        if (!fullPath.StartsWith(basePath + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
            return PreviewFileResult.BadRequest("Đường dẫn không hợp lệ");

        // 5. Fallback SPA: path không có extension → client-side route → serve index.html
        //    path có extension static mà không tồn tại → 404 thật
        if (!File.Exists(fullPath))
        {
            var requestedExt = Path.GetExtension(request.FilePath);
            var isStaticAsset = !string.IsNullOrEmpty(requestedExt)
                                && StaticAssetExtensions.Contains(requestedExt);

            if (isStaticAsset)
                return PreviewFileResult.NotFound($"File '{request.FilePath}' không tồn tại");

            var indexPath = Path.Combine(basePath, "index.html");
            if (!File.Exists(indexPath))
                return PreviewFileResult.NotFound("Không tìm thấy index.html");

            fullPath = indexPath;
        }

        var ext = Path.GetExtension(fullPath);
        var mimeType = MimeTypes.GetValueOrDefault(ext, "application/octet-stream");
        var bytes = await File.ReadAllBytesAsync(fullPath, cancellationToken);

        // 6. HTML processing
        if (ext.Equals(".html", StringComparison.OrdinalIgnoreCase) ||
            ext.Equals(".htm", StringComparison.OrdinalIgnoreCase))
        {
            var html = System.Text.Encoding.UTF8.GetString(bytes);
            var baseUrl = $"/api/preview/{request.TemplateId}";
            var baseTag = $"<base href=\"{baseUrl}/\">";

            // ═══════════════════════════════════════════════════════════════
            // VẤN ĐỀ & GIẢI PHÁP cho React Router (BrowserRouter) trong iframe
            // ═══════════════════════════════════════════════════════════════
            //
            // VẤN ĐỀ:
            //   URL thật trong iframe: /api/preview/{id}/leaderboard
            //   React Router đọc window.location.pathname = "/api/preview/{id}/leaderboard"
            //   Routes được định nghĩa: "/leaderboard", "/download", "/" ...
            //   → Không match → render NotFoundPage (404)
            //
            // TẠI SAO KHÔNG DÙNG Proxy window.location:
            //   Chrome không cho phép redefine window.location bằng defineProperty —
            //   nó là "Location" host object, throw TypeError và bị bắt bởi try/catch.
            //   Proxy không được set, native location vẫn được đọc.
            //
            // GIẢI PHÁP ĐÚNG — dùng history.replaceState() TRƯỚC KHI React chạy:
            //
            //   Script inject vào <head> KHÔNG có type="module" → chạy synchronous.
            //   Vite bundle có type="module" → defer, chạy sau.
            //   → Script chạy trước React Router hoàn toàn ✅
            //
            //   Script gọi history.replaceState(state, "", "/leaderboard"):
            //   → window.location.pathname thay đổi thành "/leaderboard" THẬT SỰ
            //   → React Router đọc pathname = "/leaderboard" → match route ✅
            //   → Không cần Proxy, không hack gì cả, dùng native API
            //
            //   Sau đó patch pushState/replaceState để khi React navigate:
            //   "/dashboard" → history ghi "/api/preview/{id}/dashboard"
            //   → F5 request lên server đúng path → server fallback index.html ✅
            //   → Script chạy lại, replaceState strip prefix → React match ✅
            //
            // FLOW HOÀN CHỈNH:
            //   1. User mở preview → iframe load /api/preview/{id}
            //   2. Script chạy: pathname = "/" → replaceState không đổi gì
            //   3. React load, router match "/" → trang chủ template ✅
            //   4. User click link /leaderboard → React gọi pushState("/leaderboard")
            //      → patch rewrite → pushState("/api/preview/{id}/leaderboard")
            //   5. React Router nhận popstate → đọc location.pathname
            //      = "/api/preview/{id}/leaderboard" → KHÔNG match
            //
            // *** ĐÂY LÀ VẤN ĐỀ CÒN LẠI: sau navigate, React Router đọc pathname mới ***
            //
            // GIẢI PHÁP THÊM — patch lại history.listen / location getter sau pushState:
            //   Khi pushState("/api/preview/{id}/leaderboard") xong, NGAY LẬP TỨC gọi
            //   replaceState lại với path đã strip → window.location.pathname = "/leaderboard"
            //   → React Router (đang trong microtask sau pushState) đọc lại → "/leaderboard" ✅
            //
            var spaFixScript = $$"""
<script>
(function () {
  var BASE = '{{baseUrl}}';

  // Strip prefix nếu có, trả về path sạch
  function stripBase(path) {
    if (path && path.indexOf(BASE) === 0)
      return path.slice(BASE.length) || '/';
    return path;
  }

  // Thêm prefix nếu cần (path tuyệt đối, chưa có prefix, không phải //)
  function addBase(path) {
    if (path && typeof path === 'string' &&
        path.charAt(0) === '/' && path.indexOf('//') !== 0 &&
        path.indexOf(BASE) !== 0)
      return BASE + path;
    return path;
  }

  // ── BƯỚC 1: Sửa URL ngay lập tức bằng replaceState ───────────────────────
  //
  // Đây là kỹ thuật duy nhất hoạt động 100% trên mọi browser:
  // Thay vì patch window.location (không được trên Chrome),
  // ta THAY ĐỔI URL THẬT bằng replaceState trước khi React chạy.
  //
  // Khi iframe load /api/preview/{id}/leaderboard:
  //   → replaceState(null, "", "/leaderboard")
  //   → window.location.pathname = "/leaderboard" (thật sự)
  //   → React Router đọc "/leaderboard" → match route ✅
  //
  var currentPath = window.location.pathname;
  var cleanPath = stripBase(currentPath);
  if (cleanPath !== currentPath) {
    // Giữ lại search và hash
    history.replaceState(
      history.state,
      '',
      cleanPath + window.location.search + window.location.hash
    );
  }

  // ── BƯỚC 2: Patch pushState/replaceState của React Router ─────────────────
  //
  // Khi React Router navigate sang "/leaderboard", nó gọi pushState(state, "", "/leaderboard").
  // Ta rewrite thành pushState(state, "", "/api/preview/{id}/leaderboard")
  // để URL thật trên thanh address đúng → F5 vẫn hoạt động.
  //
  // SAU KHI pushState xong, React Router sẽ đọc lại window.location.pathname
  // = "/api/preview/{id}/leaderboard" → lại không match.
  //
  // Fix: ngay sau pushState, gọi replaceState lại để strip prefix,
  // nhưng giữ nguyên history entry (replaceState không tạo entry mới).
  // React Router lắng nghe popstate event, không lắng nghe pushState trực tiếp,
  // nên ta có thể replaceState thoải mái mà không break navigation stack.
  //
  var _origPush = history.pushState.bind(history);
  var _origReplace = history.replaceState.bind(history);

  history.pushState = function(state, title, url) {
    // Bước 1: push với prefix đầy đủ (để server F5 hoạt động)
    var fullUrl = addBase(url);
    _origPush(state, title, fullUrl);
    // Bước 2: ngay lập tức replace để React đọc path đã strip
    var stripped = stripBase(typeof url === 'string' ? url : window.location.pathname);
    _origReplace(state, title, stripped + window.location.search + window.location.hash);
    // Bước 3: fire popstate để React Router v5/v6/v7 pick up
    window.dispatchEvent(new PopStateEvent('popstate', { state: state }));
  };

  history.replaceState = function(state, title, url) {
    var stripped = stripBase(typeof url === 'string' ? url : window.location.pathname);
    _origReplace(state, title, stripped + (window.location.search || '') + (window.location.hash || ''));
  };

  // ── BƯỚC 3: Intercept <a href="/..."> để không điều hướng sang website chính ─
  document.addEventListener('click', function(e) {
    var el = e.target.closest('a[href]');
    if (!el) return;
    var href = el.getAttribute('href');
    if (!href || !href.startsWith('/') || href.startsWith('//')) return;
    if (href.startsWith(BASE)) return;
    e.preventDefault();
    // Dùng history.pushState đã patch ở trên → tự động handle đúng
    history.pushState(null, '', href);
  }, true);

})();
</script>
""";

            // Inject vào ngay sau <head> — script này KHÔNG có type="module"
            // nên chạy synchronous, trước Vite bundle (type="module" = defer)
            if (html.Contains("<head", StringComparison.OrdinalIgnoreCase))
            {
                html = System.Text.RegularExpressions.Regex.Replace(
                    html,
                    @"(<head[^>]*>)",
                    $"$1{baseTag}{spaFixScript}",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            }
            else
            {
                html = baseTag + spaFixScript + html;
            }

            // Rewrite absolute paths trong attributes HTML
            html = System.Text.RegularExpressions.Regex.Replace(
                html,
                @"(src|href|content|action|data-src|data-href)=""(?!\/\/|https?:\/\/|#|data:|mailto:|tel:|" +
                    System.Text.RegularExpressions.Regex.Escape(baseUrl) +
                @")(\/[^""]*?)""",
                m => $"{m.Groups[1].Value}=\"{baseUrl}{m.Groups[2].Value}\"",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase
            );

            // Rewrite url('/...') trong inline style và <style>
            html = System.Text.RegularExpressions.Regex.Replace(
                html,
                @"url\(['""]?(?!\/\/|https?:\/\/|data:|" +
                    System.Text.RegularExpressions.Regex.Escape(baseUrl) +
                @")(\/[^'""\)]*?)['""]?\)",
                m => $"url(\"{baseUrl}{m.Groups[1].Value}\")",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase
            );

            // Rewrite dynamic import('/...') trong inline script
            html = System.Text.RegularExpressions.Regex.Replace(
                html,
                @"(import\s*\(['""])(?!\/\/|https?:\/\/|data:|" +
                    System.Text.RegularExpressions.Regex.Escape(baseUrl) +
                @")(\/[^'""\)]+?)(['""])",
                m => $"{m.Groups[1].Value}{baseUrl}{m.Groups[2].Value}{m.Groups[3].Value}",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase
            );

            bytes = System.Text.Encoding.UTF8.GetBytes(html);
        }

        // 7. CSS: rewrite url('/...') absolute paths
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

        // 8. JS: rewrite asset paths, dynamic import('/...') và Vite publicPath
        if (ext.Equals(".js", StringComparison.OrdinalIgnoreCase) ||
            ext.Equals(".mjs", StringComparison.OrdinalIgnoreCase))
        {
            var js = System.Text.Encoding.UTF8.GetString(bytes);
            var baseUrl = $"/api/preview/{request.TemplateId}";

            // Rewrite string literal absolute paths có extension file
            // VD: "/logo-banner.png" → "/api/preview/{id}/logo-banner.png"
            //     "/images/bg.png"   → "/api/preview/{id}/images/bg.png"
            //     "/files/game.jar"  → "/api/preview/{id}/files/game.jar"
            //
            // CHỈ rewrite path có extension file (có dấu chấm + 2-5 ký tự cuối)
            // để KHÔNG rewrite route path như "/leaderboard", "/download", "/giftcode"
            // Bỏ qua: //cdn.com, https://, đã có prefix, data:
            js = System.Text.RegularExpressions.Regex.Replace(
                js,
                @"(['""])(?!\/\/|https?:\/\/|data:|" +
                    System.Text.RegularExpressions.Regex.Escape(baseUrl) +
                @")(\/[^'""]*?\.[a-zA-Z0-9]{2,5})(['""])",
                m => $"{m.Groups[1].Value}{baseUrl}{m.Groups[2].Value}{m.Groups[3].Value}",
                System.Text.RegularExpressions.RegexOptions.None
            );

            // Rewrite dynamic import('/...')
            js = System.Text.RegularExpressions.Regex.Replace(
                js,
                @"(import\s*\(['""])(?!\/\/|https?:\/\/|data:|" +
                    System.Text.RegularExpressions.Regex.Escape(baseUrl) +
                @")(\/[^'""\)]+?)(['""])",
                m => $"{m.Groups[1].Value}{baseUrl}{m.Groups[2].Value}{m.Groups[3].Value}",
                System.Text.RegularExpressions.RegexOptions.None
            );

            // Rewrite Vite publicPath base: "/" → "/api/preview/{id}/"
            js = System.Text.RegularExpressions.Regex.Replace(
                js,
                @"(base\s*:|publicPath\s*:|__publicPath\s*=\s*)['""]\/['""]",
                m => $"{m.Groups[1].Value}\"{baseUrl}/\"",
                System.Text.RegularExpressions.RegexOptions.None
            );

            bytes = System.Text.Encoding.UTF8.GetBytes(js);
        }

        return PreviewFileResult.Success(bytes, mimeType);
    }
}