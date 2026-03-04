using MediatR;
using Qtemplate.Application.DTOs;

namespace Qtemplate.Application.Features.Tickets.Commands.ChangeTicketStatus;

public class ChangeTicketStatusCommand : IRequest<ApiResponse<object>>
{
    public int TicketId { get; set; }
    public string Status { get; set; } = string.Empty;
}