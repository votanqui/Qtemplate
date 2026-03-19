namespace Qtemplate.Application.DTOs.Community;

public class CommunityPostDto
{
    public int Id { get; set; }
    public Guid UserId { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public string? AuthorAvatar { get; set; }
    public string Content { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public int LikeCount { get; set; }
    public int CommentCount { get; set; }
    public bool IsLikedByMe { get; set; }
    public bool IsOwner { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class CommunityCommentDto
{
    public int Id { get; set; }
    public int PostId { get; set; }
    public int? ParentId { get; set; }
    public Guid UserId { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public string? AuthorAvatar { get; set; }
    public string Content { get; set; } = string.Empty;
    public bool IsOwner { get; set; }
    public List<CommunityCommentDto> Replies { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class AdminCommunityPostDto : CommunityPostDto
{
    public string AuthorEmail { get; set; } = string.Empty;
    public bool IsHidden { get; set; }
    public string? HideReason { get; set; }
}

public class AdminCommunityCommentDto : CommunityCommentDto
{
    public bool IsHidden { get; set; }
}

// ── Request DTOs ──────────────────────────────────────────────────────────────

public class CreatePostDto
{
    public string Content { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
}

public class UpdatePostDto
{
    public string Content { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
}

public class CreateCommentDto
{
    public string Content { get; set; } = string.Empty;
    public int? ParentId { get; set; }
}

public class UpdateCommentDto
{
    public string Content { get; set; } = string.Empty;
}

public class HideContentDto
{
    public bool IsHidden { get; set; }
    public string? Reason { get; set; }
}