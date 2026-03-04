using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.DTOs.Ticket;

namespace Qtemplate.Application.Features.Tickets.Commands.ReplyTicket;

public class ReplyTicketCommand : IRequest<ApiResponse<TicketReplyDto>>
{
    public int TicketId { get; set; }
    public Guid UserId { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? AttachmentUrl { get; set; }
    public bool IsFromAdmin { get; set; } = false;
}