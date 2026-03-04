using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.DTOs.Stats;
using Qtemplate.Domain.Interfaces.Repositories;

namespace Qtemplate.Application.Features.Stats.Queries.GetOrderDetailStats;

public class GetOrderDetailStatsHandler
    : IRequestHandler<GetOrderDetailStatsQuery, ApiResponse<OrderDetailStatsDto>>
{
    private readonly IStatsRepository _stats;
    public GetOrderDetailStatsHandler(IStatsRepository stats) => _stats = stats;

    public async Task<ApiResponse<OrderDetailStatsDto>> Handle(
        GetOrderDetailStatsQuery request, CancellationToken cancellationToken)
    {
        var to = (request.To ?? DateTime.UtcNow).Date.AddDays(1).AddSeconds(-1);
        var from = (request.From ?? to.AddDays(-29)).Date;
        var now = DateTime.UtcNow;
        var today = now.Date;

        var orders = await _stats.GetOrdersInRangeAsync(from, to);
        var todayOrders = await _stats.GetOrdersInRangeAsync(today, today.AddDays(1).AddSeconds(-1));
        var weekOrders = await _stats.GetOrdersInRangeAsync(
            today.AddDays(-(int)today.DayOfWeek), today.AddDays(1).AddSeconds(-1));
        var monthOrders = await _stats.GetOrdersInRangeAsync(
            new DateTime(today.Year, today.Month, 1), today.AddDays(1).AddSeconds(-1));

        // Kỳ trước so sánh
        var span = to - from;
        var prevOrders = await _stats.GetOrdersInRangeAsync(from - span, from.AddSeconds(-1));

        var paid = orders.Where(o => o.Status == "Paid").ToList();
        var cancelled = orders.Where(o => o.Status == "Cancelled").ToList();
        var prevPaid = prevOrders.Where(o => o.Status == "Paid").ToList();

        var currentRevenue = paid.Sum(o => o.FinalAmount);
        var previousRevenue = prevPaid.Sum(o => o.FinalAmount);

        var total = orders.Count;

        return ApiResponse<OrderDetailStatsDto>.Ok(new OrderDetailStatsDto
        {
            TodayRevenue = todayOrders.Where(o => o.Status == "Paid").Sum(o => o.FinalAmount),
            WeekRevenue = weekOrders.Where(o => o.Status == "Paid").Sum(o => o.FinalAmount),
            MonthRevenue = monthOrders.Where(o => o.Status == "Paid").Sum(o => o.FinalAmount),
            TodayOrders = todayOrders.Count,
            WeekOrders = weekOrders.Count(o => o.Status == "Paid"),
            MonthOrders = monthOrders.Count(o => o.Status == "Paid"),
            CompletionRate = total == 0 ? 0 : Math.Round((decimal)paid.Count / total * 100, 2),
            CancellationRate = total == 0 ? 0 : Math.Round((decimal)cancelled.Count / total * 100, 2),
            CurrentRevenue = currentRevenue,
            PreviousRevenue = previousRevenue,
            RevenueGrowth = previousRevenue == 0 ? 100
                : Math.Round((currentRevenue - previousRevenue) / previousRevenue * 100, 2),
            CurrentOrders = paid.Count,
            PreviousOrders = prevPaid.Count,
            OrderGrowth = prevPaid.Count == 0 ? 100
                : Math.Round((decimal)(paid.Count - prevPaid.Count) / prevPaid.Count * 100, 2),
            HourlyToday = Enumerable.Range(0, 24).Select(h => new HourlyStatDto
            {
                Hour = h,
                Orders = todayOrders.Count(o => o.CreatedAt.Hour == h),
                Revenue = todayOrders.Where(o => o.CreatedAt.Hour == h && o.Status == "Paid")
                                     .Sum(o => o.FinalAmount)
            }).ToList(),
            ByDay = paid
                .GroupBy(o => o.CreatedAt.ToString("yyyy-MM-dd"))
                .Select(g => new RevenueByPeriodDto
                {
                    Label = g.Key,
                    Revenue = g.Sum(o => o.FinalAmount),
                    Orders = g.Count()
                })
                .OrderBy(x => x.Label).ToList(),
            TotalOrders = total,
            PendingOrders = orders.Count(o => o.Status == "Pending"),
            PaidOrders = paid.Count,
            CancelledOrders = cancelled.Count
        });
    }
}