using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Qtemplate.Application.Features.Admin.Logs.Queries.GetAuditLogs;
using Qtemplate.Application.Features.Admin.Logs.Queries.GetEmailLogs;
using Qtemplate.Application.Features.Admin.Logs.Queries.GetRefreshTokens;
using Qtemplate.Application.Features.Admin.Logs.Queries.GetRequestLogs;

namespace Qtemplate.Controllers.Admin;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public class AdminLogController : ControllerBase
{
    private readonly IMediator _mediator;
    public AdminLogController(IMediator mediator) => _mediator = mediator;

    // GET /api/admin/request-logs?ip=&endpoint=&statusCode=&page=1
    [HttpGet("request-logs")]
    public async Task<IActionResult> GetRequestLogs(
        [FromQuery] string? ip,
        [FromQuery] string? userId,
        [FromQuery] string? endpoint,
        [FromQuery] int? statusCode,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        var result = await _mediator.Send(new GetRequestLogsQuery
        {
            Ip = ip,
            UserId = userId,
            Endpoint = endpoint,
            StatusCode = statusCode,
            Page = page,
            PageSize = pageSize
        });
        return Ok(result);
    }
    [HttpGet("audit-logs")]
    public async Task<IActionResult> GetAuditLogs(
       [FromQuery] string? userEmail,
       [FromQuery] string? action,
       [FromQuery] string? entityName,
       [FromQuery] string? entityId,
       [FromQuery] DateTime? from,
       [FromQuery] DateTime? to,
       [FromQuery] int page = 1,
       [FromQuery] int pageSize = 50)
    {
        var result = await _mediator.Send(new GetAuditLogsQuery
        {
            UserEmail = userEmail,
            Action = action,
            EntityName = entityName,
            EntityId = entityId,
            From = from,
            To = to,
            Page = page,
            PageSize = pageSize,
        });
        return Ok(result);
    }

    // GET /api/admin/email-logs?status=Failed&template=OrderConfirm
    [HttpGet("email-logs")]
    public async Task<IActionResult> GetEmailLogs(
        [FromQuery] string? status,
        [FromQuery] string? template,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _mediator.Send(new GetEmailLogsQuery
        {
            Status = status,
            Template = template,
            Page = page,
            PageSize = pageSize
        });
        return Ok(result);
    }

    // GET /api/admin/refresh-tokens?userId=&isActive=true
    [HttpGet("refresh-tokens")]
    public async Task<IActionResult> GetRefreshTokens(
        [FromQuery] Guid? userId,
        [FromQuery] bool? isActive,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _mediator.Send(new GetRefreshTokensQuery
        {
            UserId = userId,
            IsActive = isActive,
            Page = page,
            PageSize = pageSize
        });
        return Ok(result);
    }
}