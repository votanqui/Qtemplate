using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qtemplate.Application.DTOs.Template
{
    public class SaleTemplateDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string? ShortDescription { get; set; }
        public string? ThumbnailUrl { get; set; }

        // Giá
        public decimal OriginalPrice { get; set; }
        public decimal SalePrice { get; set; }
        /// <summary>% giảm, ví dụ 30 = giảm 30%</summary>
        public int DiscountPercent { get; set; }
        /// <summary>Số tiền tiết kiệm</summary>
        public decimal SaveAmount { get; set; }

        // Thời gian sale
        public DateTime? SaleStartAt { get; set; }
        public DateTime? SaleEndAt { get; set; }
        /// <summary>Sale còn hạn mãi (SaleEndAt == null)</summary>
        public bool IsOpenEnded { get; set; }

        // Meta
        public string PreviewType { get; set; } = "None";
        public int SalesCount { get; set; }
        public int ViewCount { get; set; }
        public double AverageRating { get; set; }
        public int ReviewCount { get; set; }
        public bool IsFeatured { get; set; }
        public bool IsNew { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string CategorySlug { get; set; } = string.Empty;
        public List<string> Tags { get; set; } = new();
        public bool IsInWishlist { get; set; }
    }
}
