namespace Qtemplate.Domain.Entities;

public class Template
{
    public Guid Id { get; set; }
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
    public string Status { get; set; } = "Draft";
    public string? ThumbnailUrl { get; set; }
    public string PreviewType { get; set; } = "None"; // ← THÊM: None / Iframe / ExternalUrl
    public string? PreviewFolder { get; set; }        // Dùng khi PreviewType = "Iframe"
    public string? DownloadPath { get; set; }
    public string? PreviewUrl { get; set; }           // Dùng khi PreviewType = "ExternalUrl"
    public string? TechStack { get; set; }
    public string? CompatibleWith { get; set; }
    public string? FileFormat { get; set; }
    public string? Version { get; set; }
    public bool IsFeatured { get; set; } = false;
    public bool IsNew { get; set; } = true;
    public bool IsFree { get; set; } = false;
    public int ViewCount { get; set; } = 0;
    public int SalesCount { get; set; } = 0;
    public int WishlistCount { get; set; } = 0;
    public double AverageRating { get; set; } = 0;
    public int ReviewCount { get; set; } = 0;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? PublishedAt { get; set; }
    public string StorageType { get; set; } = "Local";   // Local / GoogleDrive / S3 / R2
    public int? MediaFileId { get; set; }

    public Category Category { get; set; } = null!;
    public ICollection<TemplateImage> Images { get; set; } = new List<TemplateImage>();
    public ICollection<TemplateTag> TemplateTags { get; set; } = new List<TemplateTag>();
    public ICollection<TemplateFeature> Features { get; set; } = new List<TemplateFeature>();
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
    public ICollection<Wishlist> Wishlists { get; set; } = new List<Wishlist>();
    public ICollection<UserDownload> Downloads { get; set; } = new List<UserDownload>();
}