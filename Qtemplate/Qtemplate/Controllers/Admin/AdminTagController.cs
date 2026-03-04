using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Qtemplate.Application.DTOs.Tag;
using Qtemplate.Application.Features.Tags.Commands.CreateTag;
using Qtemplate.Application.Features.Tags.Commands.DeleteTag;
using Qtemplate.Application.Features.Tags.Commands.UpdateTag;
using Qtemplate.Application.Features.Tags.Queries.GetTags;
using System.Security.Claims;

namespace Qtemplate.Controllers.Admin;

[ApiController]
[Route("api/admin/tags")]
[Authorize(Roles = "Admin")]
public class AdminTagController : ControllerBase
{
    private readonly IMediator _mediator;
    public AdminTagController(IMediator mediator) => _mediator = mediator;

    private string? GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);
    private string? GetUserEmail() => User.FindFirstValue(ClaimTypes.Email);
    private string GetIpAddress() =>
        Request.Headers.ContainsKey("X-Forwarded-For")
            ? Request.Headers["X-Forwarded-For"].ToString()
            : HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _mediator.Send(new GetTagsQuery());
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTagDto dto)
    {
        var result = await _mediator.Send(new CreateTagCommand
        {
            Dto = dto,
            AdminId = GetUserId(),
            AdminEmail = GetUserEmail(),
            IpAddress = GetIpAddress()
        });
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] CreateTagDto dto)
    {
        var result = await _mediator.Send(new UpdateTagCommand
        {
            Id = id,
            Dto = dto,
            AdminId = GetUserId(),
            AdminEmail = GetUserEmail(),
            IpAddress = GetIpAddress()
        });
        return result.Success ? Ok(result) : BadRequest(result);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _mediator.Send(new DeleteTagCommand
        {
            Id = id,
            AdminId = GetUserId(),
            AdminEmail = GetUserEmail(),
            IpAddress = GetIpAddress()
        });
        return result.Success ? Ok(result) : BadRequest(result);
    }
}