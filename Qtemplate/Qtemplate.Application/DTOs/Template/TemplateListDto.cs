namespace Qtemplate.Application.DTOs.Template;

public class TemplateListDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? ShortDescription { get; set; }
    public string? ThumbnailUrl { get; set; }
    public decimal Price { get; set; }
    public decimal? SalePrice { get; set; }
    public bool IsFree { get; set; }
    public bool IsFeatured { get; set; }
    public bool IsNew { get; set; }
    public string PreviewType { get; set; } = "None";
    public int SalesCount { get; set; }
    public int ViewCount { get; set; }
    public double AverageRating { get; set; }
    public int ReviewCount { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string CategorySlug { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public bool IsInWishlist { get; set; }
}