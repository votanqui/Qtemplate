namespace Qtemplate.Domain.Entities;

public class TicketReply
{
    public int Id { get; set; }
    public int TicketId { get; set; }
    public Guid UserId { get; set; }                        // User hoặc Admin trả lời
    public string Message { get; set; } = string.Empty;
    public bool IsFromAdmin { get; set; } = false;
    public string? AttachmentUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation
    public SupportTicket Ticket { get; set; } = null!;
    public User User { get; set; } = null!;
}