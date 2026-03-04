namespace Qtemplate.Application.DTOs.Ticket;

public class TicketDto
{
    public int Id { get; set; }
    public string TicketCode { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public string? UserName { get; set; }
    public string? UserEmail { get; set; }
    public Guid? TemplateId { get; set; }
    public string? TemplateName { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string? AiPriorityReason { get; set; }   // ← thêm
    public string Status { get; set; } = string.Empty;
    public Guid? AssignedTo { get; set; }
    public int ReplyCount { get; set; }
    public DateTime? ClosedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class TicketDetailDto : TicketDto
{
    public List<TicketReplyDto> Replies { get; set; } = new();
}

public class TicketReplyDto
{
    public int Id { get; set; }
    public int TicketId { get; set; }
    public Guid UserId { get; set; }
    public string? UserName { get; set; }
    public string? UserAvatar { get; set; }
    public string Message { get; set; } = string.Empty;
    public bool IsFromAdmin { get; set; }
    public string? AttachmentUrl { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateTicketDto
{
    public string Subject { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public Guid? TemplateId { get; set; }
}

public class ReplyTicketDto
{
    public string Message { get; set; } = string.Empty;
    public string? AttachmentUrl { get; set; }
}

public class ChangeTicketStatusDto
{
    public string Status { get; set; } = string.Empty;
}

public class AssignTicketDto
{
    public Guid AssignedTo { get; set; }
}
public class ChangeTicketPriorityDto
{
    public string Priority { get; set; } = string.Empty;
}