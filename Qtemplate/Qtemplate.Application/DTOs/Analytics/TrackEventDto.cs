namespace Qtemplate.Application.DTOs.Analytics;

public class TrackEventDto
{
    public string PageUrl { get; set; } = string.Empty;
    public string? SessionId { get; set; }
    public string? Referer { get; set; }
    public string? UTMSource { get; set; }
    public string? UTMMedium { get; set; }
    public string? UTMCampaign { get; set; }
    public string? AffiliateCode { get; set; }
    public int TimeOnPage { get; set; } = 0;
}

public class UpdateTimeOnPageDto
{
    public string SessionId { get; set; } = string.Empty;
    public string PageUrl { get; set; } = string.Empty;
    public int Seconds { get; set; }
}