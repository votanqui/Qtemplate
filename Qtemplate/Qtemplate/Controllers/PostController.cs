using MediatR;
using Microsoft.AspNetCore.Mvc;
using Qtemplate.Application.Features.Posts.Queries.GetPostBySlug;
using Qtemplate.Application.Features.Posts.Queries.GetPublishedPosts;

namespace Qtemplate.Controllers;

[ApiController]
[Route("api/posts")]
public class PostController : ControllerBase
{
    private readonly IMediator _mediator;
    public PostController(IMediator mediator) => _mediator = mediator;


    [HttpGet]
    public async Task<IActionResult> GetList(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 12,
        [FromQuery] string? search = null,
        [FromQuery] bool? isFeatured = null)
    {
        var result = await _mediator.Send(new GetPublishedPostsQuery
        {
            Page = page,
            PageSize = pageSize,
            Search = search,
            IsFeatured = isFeatured
        });
        return Ok(result);
    }


    [HttpGet("{slug}")]
    public async Task<IActionResult> GetBySlug(string slug)
    {
        var result = await _mediator.Send(new GetPostBySlugQuery { Slug = slug });
        return result.Success ? Ok(result) : NotFound(result);
    }
}