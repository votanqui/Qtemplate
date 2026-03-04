namespace Qtemplate.Application.DTOs.Stats;

public class AnalyticsStatsDto
{
    // Tổng quan
    public int TotalPageViews { get; set; }
    public int UniqueVisitors { get; set; }  // Unique IPs
    public int UniqueUsers { get; set; }  // Logged-in users
    public double AvgTimeOnPage { get; set; }  // Giây

    // Theo thời gian
    public List<RevenueByPeriodDto> ViewsByDay { get; set; } = new();
    public List<RevenueByPeriodDto> ViewsByMonth { get; set; } = new();

    // Thiết bị / Trình duyệt / OS
    public List<BreakdownDto> ByDevice { get; set; } = new();
    public List<BreakdownDto> ByBrowser { get; set; } = new();
    public List<BreakdownDto> ByOS { get; set; } = new();

    // Top trang
    public List<TopPageDto> TopPages { get; set; } = new();

    // Nguồn traffic
    public List<BreakdownDto> ByUTMSource { get; set; } = new();
    public List<BreakdownDto> ByUTMMedium { get; set; } = new();
    public List<TopRefererDto> TopReferers { get; set; } = new();

    // Affiliate
    public List<BreakdownDto> ByAffiliateCode { get; set; } = new();
}

public class BreakdownDto
{
    public string Label { get; set; } = string.Empty;
    public int Count { get; set; }
    public double Percentage { get; set; }
}

public class TopPageDto
{
    public string PageUrl { get; set; } = string.Empty;
    public int Views { get; set; }
    public double AvgTime { get; set; }  // Giây
}

public class TopRefererDto
{
    public string Referer { get; set; } = string.Empty;
    public int Count { get; set; }
}