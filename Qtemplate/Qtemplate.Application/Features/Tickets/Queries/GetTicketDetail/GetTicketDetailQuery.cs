using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.DTOs.Ticket;

namespace Qtemplate.Application.Features.Tickets.Queries.GetTicketDetail;

public class GetTicketDetailQuery : IRequest<ApiResponse<TicketDetailDto>>
{
    public int TicketId { get; set; }
    public Guid UserId { get; set; }
    public bool IsAdmin { get; set; } = false;
}