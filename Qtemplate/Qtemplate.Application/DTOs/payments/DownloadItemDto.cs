using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qtemplate.Application.DTOs.payments
{
    public class DownloadItemDto
    {
        public Guid TemplateId { get; set; }
        public string TemplateName { get; set; } = string.Empty;
        public string DownloadUrl { get; set; } = string.Empty;  // /api/templates/{slug}/download
        public string? ThumbnailUrl { get; set; }
        public string? Slug { get; set; }
    }
    public class DownloadTemplateResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public string? FilePath { get; set; }
        public string? FileName { get; set; }
        public string ContentType { get; set; } = "application/zip";
        public string? RedirectUrl { get; set; }
        public bool IsExternal => !string.IsNullOrEmpty(RedirectUrl);
    }
}
