using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qtemplate.Application.DTOs.Template.Admin
{
    public class UpdateTemplateDto
    {
        public int CategoryId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string? ShortDescription { get; set; }
        public string? Description { get; set; }
        public string? ChangeLog { get; set; }
        public decimal Price { get; set; }
        public decimal? SalePrice { get; set; }
        public DateTime? SaleStartAt { get; set; }
        public DateTime? SaleEndAt { get; set; }
        public string? ThumbnailUrl { get; set; }
        public string? PreviewUrl { get; set; }
        public string? TechStack { get; set; }
        public string? CompatibleWith { get; set; }
        public string? FileFormat { get; set; }
        public string? Version { get; set; }
        public bool IsFeatured { get; set; }
        public bool IsFree { get; set; }
        public List<int> TagIds { get; set; } = new();
        public List<string> Features { get; set; } = new();
    }
}
