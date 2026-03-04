namespace Qtemplate.Application.DTOs.Review;

public class ReviewDto
{
    public int Id { get; set; }
    public Guid TemplateId { get; set; }
    public string? TemplateName { get; set; }
    public string? TemplateSlug { get; set; }
    public Guid UserId { get; set; }
    public string? UserName { get; set; }
    public string? UserAvatar { get; set; }
    public int Rating { get; set; }
    public string? Title { get; set; }
    public string? Comment { get; set; }
    public bool IsApproved { get; set; }
    public string? AdminReply { get; set; }
    public DateTime? AdminRepliedAt { get; set; }
    public string AiStatus { get; set; } = string.Empty;
    public string? AiReason { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateReviewDto
{
    public int Rating { get; set; }
    public string? Title { get; set; }
    public string? Comment { get; set; }
}

public class UpdateReviewDto
{
    public int Rating { get; set; }
    public string? Title { get; set; }
    public string? Comment { get; set; }
}

public class AdminReplyDto
{
    public string Reply { get; set; } = string.Empty;
}
public class ApproveReviewDto
{
    public bool IsApproved { get; set; }
}