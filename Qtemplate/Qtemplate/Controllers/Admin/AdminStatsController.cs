using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Qtemplate.Application.Features.Admin.Logs.Queries.GetRefreshTokens;
using Qtemplate.Application.Features.Stats;
using Qtemplate.Application.Features.Stats.Queries.GetCouponDetailStats;
using Qtemplate.Application.Features.Stats.Queries.GetDashboardStats;
using Qtemplate.Application.Features.Stats.Queries.GetMediaStats;
using Qtemplate.Application.Features.Stats.Queries.GetOrderDetailStats;
using Qtemplate.Application.Features.Stats.Queries.GetPaymentDetailStats;
using Qtemplate.Application.Features.Stats.Queries.GetIpBlacklist;
using Qtemplate.Application.Features.Stats.Queries.GetEmailLogs;
using Qtemplate.Application.Features.Stats.Queries.GetRequestLogs;
using Qtemplate.Application.Features.Stats.Queries.GetSecurityStats;
using Qtemplate.Application.Features.Stats.Queries.GetDailyStats;
namespace Qtemplate.Controllers.Admin;

[ApiController]
[Route("api/admin/stats")]
[Authorize(Roles = "Admin")]
public class AdminStatsController : ControllerBase
{
    private readonly IMediator _mediator;
    public AdminStatsController(IMediator mediator) => _mediator = mediator;

  
    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to)
    {
        var result = await _mediator.Send(new GetDashboardStatsQuery { From = from, To = to });
        return Ok(result);
    }


    [HttpGet("orders")]
    public async Task<IActionResult> GetOrderStats(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to)
    {
        var result = await _mediator.Send(new GetOrderDetailStatsQuery { From = from, To = to });
        return Ok(result);
    }

    [HttpGet("payments")]
    public async Task<IActionResult> GetPaymentStats(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to)
    {
        var result = await _mediator.Send(new GetPaymentDetailStatsQuery { From = from, To = to });
        return Ok(result);
    }


    [HttpGet("coupons")]
    public async Task<IActionResult> GetCouponStats()
    {
        var result = await _mediator.Send(new GetCouponDetailStatsQuery());
        return Ok(result);
    }

    [HttpGet("analytics")]
    public async Task<IActionResult> GetAnalytics(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to)
    {
        var result = await _mediator.Send(new GetAnalyticsStatsQuery { From = from, To = to });
        return Ok(result);
    }

    [HttpGet("media")]
    public async Task<IActionResult> GetMediaStats()
    {
        var result = await _mediator.Send(new GetMediaStatsQuery());
        return Ok(result);
    }

    [HttpGet("ip-blacklist-stats")]
    public async Task<IActionResult> GetIpBlacklistStats()
    {
        var result = await _mediator.Send(new GetIpBlacklistQuery());
        return Ok(result);
    }


    [HttpGet("request-log-stats")]
    public async Task<IActionResult> GetRequestLogStats(
      [FromQuery] DateTime? from,
      [FromQuery] DateTime? to)
    {
        var result = await _mediator.Send(new GetRequestLogsQuery { From = from, To = to });
        return Ok(result);
    }

    [HttpGet("email-log-stats")]
    public async Task<IActionResult> GetEmailLogStats()
    {
        var result = await _mediator.Send(new GetEmailLogsQuery());
        return Ok(result);
    }


    [HttpGet("refresh-token-stats")]
    public async Task<IActionResult> GetRefreshTokenStats()
    {
        var result = await _mediator.Send(new GetRefreshTokensQuery());
        return Ok(result);
    }
    [HttpGet("security")]
    public async Task<IActionResult> GetSecurityStats(
    [FromQuery] DateTime? from,
    [FromQuery] DateTime? to)
    {
        var result = await _mediator.Send(new GetSecurityStatsQuery { From = from, To = to });
        return Ok(result);
    }
    [HttpGet("daily")]
    public async Task<IActionResult> GetDailyStats(
    [FromQuery] string? period,
    [FromQuery] DateTime? from,
    [FromQuery] DateTime? to)
    {
        var result = await _mediator.Send(new GetDailyStatsQuery
        {
            Period = period ?? "daily",
            From = from,
            To = to,
        });
        return Ok(result);
    }
}