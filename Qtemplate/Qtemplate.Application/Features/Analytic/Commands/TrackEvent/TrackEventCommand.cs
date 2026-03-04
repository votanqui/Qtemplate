using MediatR;
using Qtemplate.Application.DTOs;

namespace Qtemplate.Application.Features.Analytic.Commands.TrackEvent;

public class TrackEventCommand : IRequest<ApiResponse<object>>
{
    public string PageUrl { get; set; } = string.Empty;
    public string? SessionId { get; set; }
    public string? UserId { get; set; }
    public string IpAddress { get; set; } = string.Empty;
    public string? Referer { get; set; }
    public string? UserAgent { get; set; }
    public string? UTMSource { get; set; }
    public string? UTMMedium { get; set; }
    public string? UTMCampaign { get; set; }
    public string? AffiliateCode { get; set; }
    public int TimeOnPage { get; set; }
}