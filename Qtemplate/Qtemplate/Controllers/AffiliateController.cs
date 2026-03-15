using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Qtemplate.Application.Features.Affiliates.Commands;
using Qtemplate.Application.Features.Affiliates.Commands.RegisterAffiliate;
using Qtemplate.Application.Features.Affiliates.Queries;
using Qtemplate.Application.Features.Affiliates.Queries.GetAffiliateStats;
using Qtemplate.Application.Features.Affiliates.Queries.GetMyTransactions;

namespace Qtemplate.Controllers;

[ApiController]
[Route("api/affiliate")]
[Authorize]
public class AffiliateController : ControllerBase
{
    private readonly IMediator _mediator;
    public AffiliateController(IMediator mediator) => _mediator = mediator;

    private Guid GetUserId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(claim, out var id) ? id : Guid.Empty;
    }

    // POST /api/affiliate/register
    [HttpPost("register")]
    public async Task<IActionResult> Register()
    {
        var userId = GetUserId();
        if (userId == Guid.Empty) return Unauthorized();

        var result = await _mediator.Send(new RegisterAffiliateCommand { UserId = userId });
        return result.Success ? Ok(result) : BadRequest(result);
    }

    // GET /api/affiliate/stats
    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
    {
        var userId = GetUserId();
        if (userId == Guid.Empty) return Unauthorized();

        var result = await _mediator.Send(new GetAffiliateStatsQuery { UserId = userId });
        return result.Success ? Ok(result) : BadRequest(result);
    }
    [HttpGet("transactions")]
    public async Task<IActionResult> GetMyTransactions(
    [FromQuery] string? status,
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 20)
    {
        var userId = GetUserId();
        if (userId == Guid.Empty) return Unauthorized();

        var result = await _mediator.Send(new GetMyAffiliateTransactionsQuery
        {
            UserId = userId,
            Status = status,
            Page = page,
            PageSize = pageSize,
        });
        return result.Success ? Ok(result) : BadRequest(result);
    }
}