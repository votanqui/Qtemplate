namespace Qtemplate.Domain.Entities;

public class Post
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;        
    public string? Excerpt { get; set; }                     
    public string Content { get; set; } = string.Empty;      
    public string? ThumbnailUrl { get; set; }                 
    public string Status { get; set; } = "Draft";             // Draft / Published / Archived
    public bool IsFeatured { get; set; } = false;
    public int ViewCount { get; set; } = 0;
    public int SortOrder { get; set; } = 0;

    // Tác giả (Admin user)
    public string AuthorId { get; set; } = string.Empty;
    public string AuthorName { get; set; } = string.Empty;

    // SEO
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }

    public DateTime? PublishedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;


    public string? Tags { get; set; } // lưu dạng JSON array string để đơn giản
}