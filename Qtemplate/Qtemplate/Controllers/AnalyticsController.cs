using MediatR;
using Microsoft.AspNetCore.Mvc;
using Qtemplate.Application.DTOs.Analytics;
using Qtemplate.Application.Features.Analytic.Commands.TrackEvent;
using Qtemplate.Application.Features.Analytic.Commands.UpdateTimeOnPage;
using System.Security.Claims;

namespace Qtemplate.Controllers;

[ApiController]
[Route("api/analytics")]
public class AnalyticsController : ControllerBase
{
    private readonly IMediator _mediator;
    public AnalyticsController(IMediator mediator) => _mediator = mediator;

    private string GetIp() =>
        Request.Headers["X-Forwarded-For"].FirstOrDefault()
        ?? HttpContext.Connection.RemoteIpAddress?.ToString()
        ?? "unknown";

    // POST /api/analytics/track
    // Frontend gọi khi user vào trang
    [HttpPost("track")]
    public async Task<IActionResult> Track([FromBody] TrackEventDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var ua = Request.Headers.UserAgent.ToString();

        var result = await _mediator.Send(new TrackEventCommand
        {
            PageUrl = dto.PageUrl,
            SessionId = dto.SessionId,
            UserId = userId,
            IpAddress = GetIp(),
            Referer = dto.Referer,
            UserAgent = ua,
            UTMSource = dto.UTMSource,
            UTMMedium = dto.UTMMedium,
            UTMCampaign = dto.UTMCampaign,
            AffiliateCode = dto.AffiliateCode,
            TimeOnPage = dto.TimeOnPage
        });

        return Ok(result);
    }

    // PATCH /api/analytics/time-on-page
    // Frontend gọi khi user rời trang (beforeunload)
    [HttpPatch("time-on-page")]
    public async Task<IActionResult> UpdateTime([FromBody] UpdateTimeOnPageDto dto)
    {
        var result = await _mediator.Send(new UpdateTimeOnPageCommand
        {
            SessionId = dto.SessionId,
            PageUrl = dto.PageUrl,
            Seconds = dto.Seconds
        });
        return Ok(result);
    }
}