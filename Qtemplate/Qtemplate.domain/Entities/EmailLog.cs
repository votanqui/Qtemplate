namespace Qtemplate.Domain.Entities;

public class EmailLog
{
    public long Id { get; set; }
    public string To { get; set; } = string.Empty;
    public string? Cc { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string Template { get; set; } = string.Empty;    // "OrderConfirm" / "ResetPassword"
    public string Status { get; set; } = "Pending";         // Pending / Sent / Failed
    public string? ErrorMessage { get; set; }
    public int RetryCount { get; set; } = 0;
    public DateTime? SentAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}