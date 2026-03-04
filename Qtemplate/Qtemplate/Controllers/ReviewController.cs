using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Qtemplate.Application.DTOs.Review;
using Qtemplate.Application.Features.Reviews.Commands.CreateReview;
using Qtemplate.Application.Features.Reviews.Commands.DeleteReview;
using Qtemplate.Application.Features.Reviews.Commands.UpdateReview;
using Qtemplate.Application.Features.Reviews.Queries.GetMyReviews;
using Qtemplate.Application.Features.Reviews.Queries.GetTemplateReviews;

namespace Qtemplate.Controllers;

[ApiController]
[Route("api")]
public class ReviewController : ControllerBase
{
    private readonly IMediator _mediator;
    public ReviewController(IMediator mediator) => _mediator = mediator;

    private Guid GetUserId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(claim, out var id) ? id : Guid.Empty;
    }

    // GET /api/templates/{slug}/reviews
    [HttpGet("templates/{slug}/reviews")]
    public async Task<IActionResult> GetTemplateReviews(
        string slug,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await _mediator.Send(new GetTemplateReviewsQuery
        {
            Slug = slug,
            Page = page,
            PageSize = pageSize
        });
        return Ok(result);
    }

    // POST /api/templates/{slug}/reviews
    [HttpPost("templates/{slug}/reviews")]
    [Authorize]
    public async Task<IActionResult> CreateReview(string slug, [FromBody] CreateReviewDto dto)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty) return Unauthorized();

        var result = await _mediator.Send(new CreateReviewCommand
        {
            Slug = slug,
            UserId = userId,
            Rating = dto.Rating,
            Title = dto.Title,
            Comment = dto.Comment
        });
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // GET /api/user/reviews
    [HttpGet("user/reviews")]
    [Authorize]
    public async Task<IActionResult> GetMyReviews()
    {
        var userId = GetUserId();
        if (userId == Guid.Empty) return Unauthorized();

        var result = await _mediator.Send(new GetMyReviewsQuery { UserId = userId });
        return Ok(result);
    }

    // PUT /api/user/reviews/{id}
    [HttpPut("user/reviews/{id:int}")]
    [Authorize]
    public async Task<IActionResult> UpdateReview(int id, [FromBody] UpdateReviewDto dto)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty) return Unauthorized();

        var result = await _mediator.Send(new UpdateReviewCommand
        {
            ReviewId = id,
            UserId = userId,
            Rating = dto.Rating,
            Title = dto.Title,
            Comment = dto.Comment
        });
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // DELETE /api/user/reviews/{id}
    [HttpDelete("user/reviews/{id:int}")]
    [Authorize]
    public async Task<IActionResult> DeleteReview(int id)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty) return Unauthorized();

        var result = await _mediator.Send(new DeleteReviewCommand
        {
            ReviewId = id,
            UserId = userId,
            IsAdmin = false
        });
        return result.Success ? Ok(result) : BadRequest(result);
    }
}