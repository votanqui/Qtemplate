using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Qtemplate.Application.DTOs.IpBlacklist;

using Qtemplate.Application.Features.Admin.IpBlacklist.Commands.AddIpBlacklist;
using Qtemplate.Application.Features.Admin.IpBlacklist.Commands.DeleteIpBlacklist;
using Qtemplate.Application.Features.Admin.IpBlacklist.Commands.ToggleIpBlacklist;
using Qtemplate.Application.Features.Admin.IpBlacklist.Queries.GetIpBlacklist;

namespace Qtemplate.Controllers.Admin;

[ApiController]
[Route("api/admin/ip-blacklist")]
[Authorize(Roles = "Admin")]
public class AdminIpBlacklistController : ControllerBase
{
    private readonly IMediator _mediator;
    public AdminIpBlacklistController(IMediator mediator) => _mediator = mediator;

    private string? GetAdminEmail() => User.FindFirstValue(ClaimTypes.Email);

    // GET /api/admin/ip-blacklist?page=1
    [HttpGet]
    public async Task<IActionResult> GetList(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _mediator.Send(new GetIpBlacklistQuery
        {
            Page = page,
            PageSize = pageSize
        });
        return Ok(result);
    }

    // POST /api/admin/ip-blacklist
    [HttpPost]
    public async Task<IActionResult> Add([FromBody] AddIpBlacklistDto dto)
    {
        var result = await _mediator.Send(new AddIpBlacklistCommand
        {
            IpAddress = dto.IpAddress,
            Reason = dto.Reason,
            ExpiredAt = dto.ExpiredAt,
            AdminEmail = GetAdminEmail()
        });
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // PATCH /api/admin/ip-blacklist/{id}/toggle
    [HttpPatch("{id:int}/toggle")]
    public async Task<IActionResult> Toggle(int id)
    {
        var result = await _mediator.Send(new ToggleIpBlacklistCommand { Id = id });
        return result.Success ? Ok(result) : NotFound(result);
    }

    // DELETE /api/admin/ip-blacklist/{id}
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _mediator.Send(new DeleteIpBlacklistCommand { Id = id });
        return result.Success ? Ok(result) : NotFound(result);
    }
}