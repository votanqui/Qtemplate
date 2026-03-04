namespace Qtemplate.Domain.Entities;

public class Analytics
{
    public long Id { get; set; }
    public string? SessionId { get; set; }
    public string? UserId { get; set; }
    public string IpAddress { get; set; } = string.Empty;
    public string? Country { get; set; }
    public string? City { get; set; }
    public string? Device { get; set; }                     // Desktop / Mobile / Tablet
    public string? Browser { get; set; }
    public string? OS { get; set; }
    public string PageUrl { get; set; } = string.Empty;
    public string? Referer { get; set; }
    public string? UTMSource { get; set; }                  // google / facebook
    public string? UTMMedium { get; set; }                  // cpc / email
    public string? UTMCampaign { get; set; }
    public string? AffiliateCode { get; set; }
    public int TimeOnPage { get; set; } = 0;                // Giây
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}