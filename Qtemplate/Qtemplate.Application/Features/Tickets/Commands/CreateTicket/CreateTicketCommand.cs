using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.DTOs.Ticket;

namespace Qtemplate.Application.Features.Tickets.Commands.CreateTicket;

public class CreateTicketCommand : IRequest<ApiResponse<TicketDto>>
{
    public Guid UserId { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Priority { get; set; } = "Normal";
    public Guid? TemplateId { get; set; }
}