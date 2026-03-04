using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Qtemplate.Application.DTOs.User;
using Qtemplate.Application.Features.Admin.Users.Commands.ChangeUserRole;
using Qtemplate.Application.Features.Admin.Users.Commands.ChangeUserStatus;
using Qtemplate.Application.Features.Admin.Users.Queries.GetUserDetail;
using Qtemplate.Application.Features.Admin.Users.Queries.GetUserList;
using Qtemplate.Application.Features.Admin.Users.Queries.GetUserOrders;

namespace Qtemplate.Controllers.Admin;

[ApiController]
[Route("api/admin/users")]
[Authorize(Roles = "Admin")]
public class AdminUserController : ControllerBase
{
    private readonly IMediator _mediator;
    public AdminUserController(IMediator mediator) => _mediator = mediator;

    private string? GetAdminId() => User.FindFirstValue(ClaimTypes.NameIdentifier);
    private string? GetAdminEmail() => User.FindFirstValue(ClaimTypes.Email);
    private string GetIpAddress() =>
        Request.Headers.ContainsKey("X-Forwarded-For")
            ? Request.Headers["X-Forwarded-For"].ToString()
            : HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

    // GET /api/admin/users?search=&role=&isActive=&page=1&pageSize=20
    [HttpGet]
    public async Task<IActionResult> GetList(
        [FromQuery] string? search,
        [FromQuery] string? role,
        [FromQuery] bool? isActive,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _mediator.Send(new GetUserListQuery
        {
            Search = search,
            Role = role,
            IsActive = isActive,
            Page = page,
            PageSize = pageSize
        });
        return Ok(result);
    }

    // GET /api/admin/users/{id}
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetDetail(Guid id)
    {
        var result = await _mediator.Send(new GetUserDetailQuery { UserId = id });
        return result.Success ? Ok(result) : NotFound(result);
    }

    // PATCH /api/admin/users/{id}/status
    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> ChangeStatus(Guid id, [FromBody] ChangeUserStatusDto dto)
    {
        var result = await _mediator.Send(new ChangeUserStatusCommand
        {
            TargetUserId = id,
            IsActive = dto.IsActive,
            Reason = dto.Reason,
            AdminId = GetAdminId(),
            AdminEmail = GetAdminEmail(),
            IpAddress = GetIpAddress()
        });
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // PATCH /api/admin/users/{id}/role
    [HttpPatch("{id:guid}/role")]
    public async Task<IActionResult> ChangeRole(Guid id, [FromBody] ChangeUserRoleDto dto)
    {
        var result = await _mediator.Send(new ChangeUserRoleCommand
        {
            TargetUserId = id,
            Role = dto.Role,
            AdminId = GetAdminId(),
            AdminEmail = GetAdminEmail(),
            IpAddress = GetIpAddress()
        });
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // GET /api/admin/users/{id}/orders?page=1&pageSize=10
    [HttpGet("{id:guid}/orders")]
    public async Task<IActionResult> GetUserOrders(
        Guid id,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await _mediator.Send(new GetUserOrdersQuery
        {
            UserId = id,
            Page = page,
            PageSize = pageSize
        });
        return result.Success ? Ok(result) : NotFound(result);
    }
}