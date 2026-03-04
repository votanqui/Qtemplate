using MediatR;
using Qtemplate.Application.DTOs;

namespace Qtemplate.Application.Features.Tickets.Commands.ChangeTicketPriority;

public class ChangeTicketPriorityCommand : IRequest<ApiResponse<object>>
{
    public int TicketId { get; set; }
    public string Priority { get; set; } = string.Empty;
}