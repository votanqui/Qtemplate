using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.DTOs.Stats;
using Qtemplate.Domain.Interfaces.Repositories;

namespace Qtemplate.Application.Features.Stats.Queries.GetAnalyticsStats;

public class GetAnalyticsStatsHandler
    : IRequestHandler<GetAnalyticsStatsQuery, ApiResponse<AnalyticsStatsDto>>
{
    private readonly IStatsRepository _stats;
    public GetAnalyticsStatsHandler(IStatsRepository stats) => _stats = stats;

    public async Task<ApiResponse<AnalyticsStatsDto>> Handle(
        GetAnalyticsStatsQuery request, CancellationToken cancellationToken)
    {
        var to = (request.To ?? DateTime.UtcNow).Date.AddDays(1).AddSeconds(-1);
        var from = (request.From ?? to.AddDays(-29)).Date;

        var data = await _stats.GetAnalyticsInRangeAsync(from, to);
        if (!data.Any())
            return ApiResponse<AnalyticsStatsDto>.Ok(new AnalyticsStatsDto());

        var total = data.Count;

        return ApiResponse<AnalyticsStatsDto>.Ok(new AnalyticsStatsDto
        {
            TotalPageViews = total,
            UniqueVisitors = data.Select(a => a.IpAddress).Distinct().Count(),
            UniqueUsers = data.Where(a => a.UserId != null).Select(a => a.UserId).Distinct().Count(),
            AvgTimeOnPage = Math.Round(data.Average(a => a.TimeOnPage), 1),
            ViewsByDay = data
                .GroupBy(a => a.CreatedAt.ToString("yyyy-MM-dd"))
                .Select(g => new RevenueByPeriodDto { Label = g.Key, Orders = g.Count() })
                .OrderBy(x => x.Label).ToList(),
            ViewsByMonth = data
                .GroupBy(a => a.CreatedAt.ToString("yyyy-MM"))
                .Select(g => new RevenueByPeriodDto { Label = g.Key, Orders = g.Count() })
                .OrderBy(x => x.Label).ToList(),
            ByDevice = Breakdown(data.Select(a => a.Device ?? "Unknown"), total),
            ByBrowser = Breakdown(data.Select(a => a.Browser ?? "Unknown"), total),
            ByOS = Breakdown(data.Select(a => a.OS ?? "Unknown"), total),
            TopPages = data
                .GroupBy(a => a.PageUrl)
                .Select(g => new TopPageDto
                {
                    PageUrl = g.Key,
                    Views = g.Count(),
                    AvgTime = g.Average(a => a.TimeOnPage)
                })
                .OrderByDescending(x => x.Views).Take(20).ToList(),
            ByUTMSource = Breakdown(data.Where(a => a.UTMSource != null).Select(a => a.UTMSource!), total),
            ByUTMMedium = Breakdown(data.Where(a => a.UTMMedium != null).Select(a => a.UTMMedium!), total),
            TopReferers = data
                .Where(a => !string.IsNullOrEmpty(a.Referer))
                .GroupBy(a => a.Referer!)
                .Select(g => new TopRefererDto { Referer = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count).Take(10).ToList(),
            ByAffiliateCode = Breakdown(data.Where(a => a.AffiliateCode != null).Select(a => a.AffiliateCode!), total)
        });
    }

    private static List<BreakdownDto> Breakdown(IEnumerable<string> values, int total) =>
        values
            .GroupBy(v => v)
            .Select(g => new BreakdownDto
            {
                Label = g.Key,
                Count = g.Count(),
                Percentage = total == 0 ? 0 : Math.Round((double)g.Count() / total * 100, 2)
            })
            .OrderByDescending(x => x.Count).ToList();
}