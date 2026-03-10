using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Qtemplate.Application.Features.Admin.Notifications.Commands.SendNotification;

namespace Qtemplate.Controllers.Admin;

[ApiController]
[Route("api/admin/notifications")]
[Authorize(Roles = "Admin")]
public class AdminNotificationController : ControllerBase
{
    private readonly IMediator _mediator;
    public AdminNotificationController(IMediator mediator) => _mediator = mediator;

    /// <summary>Admin gửi thông báo realtime</summary>
    [HttpPost("send")]
    public async Task<IActionResult> Send([FromBody] SendNotificationCommand command)
    {
        var result = await _mediator.Send(command);
        return result.Success ? Ok(result) : BadRequest(result);
    }
}