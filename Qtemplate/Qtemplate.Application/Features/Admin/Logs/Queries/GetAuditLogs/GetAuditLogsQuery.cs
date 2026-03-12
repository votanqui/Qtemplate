using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.DTOs.AuditLog;

namespace Qtemplate.Application.Features.Admin.Logs.Queries.GetAuditLogs;

public class GetAuditLogsQuery : IRequest<ApiResponse<PaginatedResult<AuditLogDto>>>
{
    public string? UserEmail { get; set; }
    public string? Action { get; set; }
    public string? EntityName { get; set; }
    public string? EntityId { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}