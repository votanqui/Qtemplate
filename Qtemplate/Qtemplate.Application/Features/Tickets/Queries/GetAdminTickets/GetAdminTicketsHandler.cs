using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.DTOs.Ticket;
using Qtemplate.Application.Mappers;
using Qtemplate.Domain.Interfaces.Repositories;

namespace Qtemplate.Application.Features.Tickets.Queries.GetAdminTickets;

public class GetAdminTicketsHandler
    : IRequestHandler<GetAdminTicketsQuery, ApiResponse<PaginatedResult<TicketDto>>>
{
    private readonly ITicketRepository _ticketRepo;

    public GetAdminTicketsHandler(ITicketRepository ticketRepo) => _ticketRepo = ticketRepo;

    public async Task<ApiResponse<PaginatedResult<TicketDto>>> Handle(
        GetAdminTicketsQuery request, CancellationToken cancellationToken)
    {
        var (items, total) = await _ticketRepo.GetAdminListAsync(
            request.Status, request.Priority, request.Page, request.PageSize);

        return ApiResponse<PaginatedResult<TicketDto>>.Ok(new PaginatedResult<TicketDto>
        {
            Items = items.Select(TicketMapper.ToDto).ToList(),
            TotalCount = total,
            Page = request.Page,
            PageSize = request.PageSize
        });
    }
}