using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.DTOs.AuditLog;
using Qtemplate.Domain.Interfaces.Repositories;

namespace Qtemplate.Application.Features.Admin.Logs.Queries.GetAuditLogs;

public class GetAuditLogsHandler
    : IRequestHandler<GetAuditLogsQuery, ApiResponse<PaginatedResult<AuditLogDto>>>
{
    private readonly IAuditLogRepository _repo;
    public GetAuditLogsHandler(IAuditLogRepository repo) => _repo = repo;

    public async Task<ApiResponse<PaginatedResult<AuditLogDto>>> Handle(
        GetAuditLogsQuery request, CancellationToken cancellationToken)
    {
        var (items, total) = await _repo.GetPagedAsync(
            request.UserEmail,
            request.Action,
            request.EntityName,
            request.EntityId,
            request.From,
            request.To,
            request.Page,
            request.PageSize);

        return ApiResponse<PaginatedResult<AuditLogDto>>.Ok(new PaginatedResult<AuditLogDto>
        {
            Items = items.Select(a => new AuditLogDto
            {
                Id = a.Id,
                UserId = a.UserId,
                UserEmail = a.UserEmail,
                Action = a.Action,
                EntityName = a.EntityName,
                EntityId = a.EntityId,
                OldValues = a.OldValues,
                NewValues = a.NewValues,
                IpAddress = a.IpAddress,
                UserAgent = a.UserAgent,
                CreatedAt = a.CreatedAt,
            }).ToList(),
            TotalCount = total,
            Page = request.Page,
            PageSize = request.PageSize,
        });
    }
}