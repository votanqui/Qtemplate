namespace Qtemplate.Application.DTOs.Template;

public class TemplateDetailDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? ShortDescription { get; set; }
    public string? Description { get; set; }
    public string? ChangeLog { get; set; }
    public decimal Price { get; set; }
    public decimal? SalePrice { get; set; }
    public DateTime? SaleStartAt { get; set; }
    public DateTime? SaleEndAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public string PreviewType { get; set; } = "None";
    public string? ThumbnailUrl { get; set; }
    public string? PreviewFolder { get; set; }
    public string? DownloadPath { get; set; }
    public string? PreviewUrl { get; set; }
    public string? TechStack { get; set; }
    public string? CompatibleWith { get; set; }
    public string? FileFormat { get; set; }
    public string? Version { get; set; }
    public bool IsFeatured { get; set; }
    public bool IsNew { get; set; }
    public bool IsFree { get; set; }
    public int ViewCount { get; set; }
    public int SalesCount { get; set; }
    public int WishlistCount { get; set; }
    public double AverageRating { get; set; }
    public int ReviewCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? PublishedAt { get; set; }

    // Relations
    public TemplateCategoryDto Category { get; set; } = new();
    public List<string> Tags { get; set; } = new();
    public List<string> Features { get; set; } = new();
    public List<TemplateImageDto> Images { get; set; } = new();

    // Trạng thái với user hiện tại
    public bool IsInWishlist { get; set; }
    public bool IsPurchased { get; set; }
}

public class TemplateCategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
}

public class TemplateImageDto
{
    public string ImageUrl { get; set; } = string.Empty;
    public string? AltText { get; set; }
    public string Type { get; set; } = string.Empty;
    public int SortOrder { get; set; }
}