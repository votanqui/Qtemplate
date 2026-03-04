using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Qtemplate.Application.DTOs.Review;
using Qtemplate.Application.Features.Reviews.Commands.AdminApproveReview;
using Qtemplate.Application.Features.Reviews.Commands.AdminReplyReview;
using Qtemplate.Application.Features.Reviews.Commands.DeleteReview;
using Qtemplate.Application.Features.Reviews.Queries.GetAdminReviews;

namespace Qtemplate.Controllers.Admin;

[ApiController]
[Route("api/admin/reviews")]
[Authorize(Roles = "Admin")]
public class AdminReviewController : ControllerBase
{
    private readonly IMediator _mediator;
    public AdminReviewController(IMediator mediator) => _mediator = mediator;

    private Guid GetUserId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(claim, out var id) ? id : Guid.Empty;
    }

    // GET /api/admin/reviews?status=pending&page=1
    [HttpGet]
    public async Task<IActionResult> GetList(
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var result = await _mediator.Send(new GetAdminReviewsQuery
        {
            Status = status,
            Page = page,
            PageSize = pageSize
        });
        return Ok(result);
    }

    // PATCH /api/admin/reviews/{id}/approve
    [HttpPatch("{id:int}/approve")]
    public async Task<IActionResult> Approve(int id, [FromBody] ApproveReviewDto dto)
    {
        var result = await _mediator.Send(new AdminApproveReviewCommand
        {
            ReviewId = id,
            IsApproved = dto.IsApproved
        });
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // PATCH /api/admin/reviews/{id}/reply
    [HttpPatch("{id:int}/reply")]
    public async Task<IActionResult> Reply(int id, [FromBody] AdminReplyDto dto)
    {
        var result = await _mediator.Send(new AdminReplyReviewCommand
        {
            ReviewId = id,
            Reply = dto.Reply
        });
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // DELETE /api/admin/reviews/{id}
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _mediator.Send(new DeleteReviewCommand
        {
            ReviewId = id,
            UserId = GetUserId(),
            IsAdmin = true
        });
        return result.Success ? Ok(result) : BadRequest(result);
    }
}