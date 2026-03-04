using Qtemplate.Application.DTOs.Ticket;
using Qtemplate.Domain.Entities;

namespace Qtemplate.Application.Mappers;

public static class TicketMapper
{
    public static TicketDto ToDto(SupportTicket t) => new()
    {
        Id = t.Id,
        TicketCode = t.TicketCode,
        UserId = t.UserId,
        UserName = t.User?.FullName,
        UserEmail = t.User?.Email,
        TemplateId = t.TemplateId,
        TemplateName = t.Template?.Name,
        Subject = t.Subject,
        Message = t.Message,
        Priority = t.Priority,
        AiPriorityReason = t.AiPriorityReason,   // ← thêm
        Status = t.Status,
        AssignedTo = t.AssignedTo,
        ReplyCount = t.Replies?.Count ?? 0,
        ClosedAt = t.ClosedAt,
        CreatedAt = t.CreatedAt,
        UpdatedAt = t.UpdatedAt
    };

    public static TicketDetailDto ToDetailDto(SupportTicket t) => new()
    {
        Id = t.Id,
        TicketCode = t.TicketCode,
        UserId = t.UserId,
        UserName = t.User?.FullName,
        UserEmail = t.User?.Email,
        TemplateId = t.TemplateId,
        TemplateName = t.Template?.Name,
        Subject = t.Subject,
        Message = t.Message,
        Priority = t.Priority,
        AiPriorityReason = t.AiPriorityReason,   // ← thêm
        Status = t.Status,
        AssignedTo = t.AssignedTo,
        ReplyCount = t.Replies?.Count ?? 0,
        ClosedAt = t.ClosedAt,
        CreatedAt = t.CreatedAt,
        UpdatedAt = t.UpdatedAt,
        Replies = t.Replies?
            .OrderBy(r => r.CreatedAt)
            .Select(ToReplyDto)
            .ToList() ?? new()
    };

    public static TicketReplyDto ToReplyDto(TicketReply r) => new()
    {
        Id = r.Id,
        TicketId = r.TicketId,
        UserId = r.UserId,
        UserName = r.User?.FullName,
        UserAvatar = r.User?.AvatarUrl,
        Message = r.Message,
        IsFromAdmin = r.IsFromAdmin,
        AttachmentUrl = r.AttachmentUrl,
        CreatedAt = r.CreatedAt
    };
}