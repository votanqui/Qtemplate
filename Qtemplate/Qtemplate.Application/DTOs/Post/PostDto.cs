namespace Qtemplate.Application.DTOs.Post;

// DTO trả về cho public (list)
public class PostListDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Excerpt { get; set; }
    public string? ThumbnailUrl { get; set; }
    public bool IsFeatured { get; set; }
    public int ViewCount { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public string? Tags { get; set; }
    public DateTime? PublishedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

// DTO trả về cho public (detail) - thêm nội dung đầy đủ
public class PostDetailDto : PostListDto
{
    public string Content { get; set; } = string.Empty;
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public DateTime UpdatedAt { get; set; }
}

// DTO trả về cho Admin (có thêm Status)
public class AdminPostDto : PostDetailDto
{
    public string Status { get; set; } = string.Empty;
    public int SortOrder { get; set; }
}

// DTO tạo / cập nhật bài viết
public class UpsertPostDto
{
    public string Title { get; set; } = string.Empty;
    public string? Slug { get; set; }                // tự gen nếu null
    public string? Excerpt { get; set; }
    public string Content { get; set; } = string.Empty;
    public string? ThumbnailUrl { get; set; }
    public string Status { get; set; } = "Draft";    // Draft / Published / Archived
    public bool IsFeatured { get; set; } = false;
    public int SortOrder { get; set; } = 0;
    public string? Tags { get; set; }
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public DateTime? PublishedAt { get; set; }
}