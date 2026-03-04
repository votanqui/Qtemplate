namespace Qtemplate.Domain.Entities;

public class SupportTicket
{
    public int Id { get; set; }
    public Guid UserId { get; set; }
    public Guid? TemplateId { get; set; }                   // Ticket liên quan template nào
    public string TicketCode { get; set; } = string.Empty;  // "TK-20240101-001"
    public string Subject { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Priority { get; set; } = "Normal";        // Low / Normal / High / Urgent
    public string Status { get; set; } = "Open";            // Open / InProgress / Closed
    public Guid? AssignedTo { get; set; }                   // Admin xử lý
    public DateTime? ClosedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public string? AiPriorityReason { get; set; }   // lý do AI phân loại
    public bool AiProcessed { get; set; } = false;
    // Navigation
    public User User { get; set; } = null!;
    public Template? Template { get; set; }
    public ICollection<TicketReply> Replies { get; set; } = new List<TicketReply>();
}