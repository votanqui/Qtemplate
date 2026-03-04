using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.DTOs.Ticket;

namespace Qtemplate.Application.Features.Tickets.Queries.GetMyTickets;

public class GetMyTicketsQuery : IRequest<ApiResponse<PaginatedResult<TicketDto>>>
{
    public Guid UserId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}