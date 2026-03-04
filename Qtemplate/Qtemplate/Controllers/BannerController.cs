using MediatR;
using Microsoft.AspNetCore.Mvc;
using Qtemplate.Application.Features.Banners.Queries;
using Qtemplate.Application.Features.Banners.Queries.GetBanner;

namespace Qtemplate.Controllers;

[ApiController]
[Route("api/banners")]
public class BannerController : ControllerBase
{
    private readonly IMediator _mediator;
    public BannerController(IMediator mediator) => _mediator = mediator;

    // GET /api/banners?position=Home
    [HttpGet]
    public async Task<IActionResult> GetBanners([FromQuery] string? position)
    {
        var result = await _mediator.Send(new GetBannersQuery { Position = position });
        return Ok(result);
    }
}