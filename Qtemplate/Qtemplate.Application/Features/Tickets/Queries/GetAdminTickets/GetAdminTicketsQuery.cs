using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.DTOs.Ticket;

namespace Qtemplate.Application.Features.Tickets.Queries.GetAdminTickets;

public class GetAdminTicketsQuery : IRequest<ApiResponse<PaginatedResult<TicketDto>>>
{
    public string? Status { get; set; }
    public string? Priority { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}