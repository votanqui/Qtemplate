namespace Qtemplate.Domain.Entities;

public class Review
{
    public int Id { get; set; }
    public Guid TemplateId { get; set; }
    public Guid UserId { get; set; }
    public Guid? OrderItemId { get; set; }
    public int Rating { get; set; }
    public string? Title { get; set; }
    public string? Comment { get; set; }
    public bool IsApproved { get; set; } = false;
    public string? AdminReply { get; set; }
    public DateTime? AdminRepliedAt { get; set; }

    // AI moderation
    public string AiStatus { get; set; } = "Pending";  // Pending / Approved / Rejected
    public string? AiReason { get; set; }               // lý do AI reject

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation
    public Template Template { get; set; } = null!;
    public User User { get; set; } = null!;
}