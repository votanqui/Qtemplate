using MediatR;
using Qtemplate.Application.DTOs;

namespace Qtemplate.Application.Features.Tickets.Commands.AssignTicket;

public class AssignTicketCommand : IRequest<ApiResponse<object>>
{
    public int TicketId { get; set; }
    public Guid AssignedTo { get; set; }
}