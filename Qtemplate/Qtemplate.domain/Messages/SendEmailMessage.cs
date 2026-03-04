namespace Qtemplate.Domain.Messages;

public class SendEmailMessage
{
    public string To { get; set; } = string.Empty;
    public string? Cc { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string Template { get; set; } = string.Empty;
    public long? EmailLogId { get; set; }
}