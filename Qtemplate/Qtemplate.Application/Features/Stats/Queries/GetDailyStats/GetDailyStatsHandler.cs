using MediatR;
using Qtemplate.Application.DTOs.Stats;
using Qtemplate.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Qtemplate.Domain.Interfaces.Repositories;

namespace Qtemplate.Application.Features.Stats.Queries.GetDailyStats
{
    public class GetDailyStatsHandler : IRequestHandler<GetDailyStatsQuery, ApiResponse<DailyStatsResultDto>>
    {
        private readonly IStatsRepository _statsRepo;
        public GetDailyStatsHandler(IStatsRepository statsRepo) => _statsRepo = statsRepo;

        public async Task<ApiResponse<DailyStatsResultDto>> Handle(
            GetDailyStatsQuery request, CancellationToken cancellationToken)
        {
            var period = (request.Period ?? "daily").ToLowerInvariant();
            var to = (request.To ?? DateTime.UtcNow).Date;
            var from = request.From?.Date ?? period switch
            {
                "monthly" => to.AddMonths(-11).AddDays(1 - to.Day),
                "weekly" => to.AddDays(-(int)to.DayOfWeek).AddDays(-7 * 11),
                _ => to.AddDays(-29)
            };

            var rows = await _statsRepo.GetDailyStatsAsync(from, to);

            IEnumerable<DailyStatDto> items = period switch
            {
                "monthly" => rows
                    .GroupBy(r => new { r.Date.Year, r.Date.Month })
                    .Select(g => new DailyStatDto
                    {
                        Date = new DateTime(g.Key.Year, g.Key.Month, 1),
                        DateLabel = $"T{g.Key.Month}/{g.Key.Year}",
                        TotalOrders = g.Sum(r => r.TotalOrders),
                        PaidOrders = g.Sum(r => r.PaidOrders),
                        CancelledOrders = g.Sum(r => r.CancelledOrders),
                        TotalRevenue = g.Sum(r => r.TotalRevenue),
                        NewUsers = g.Sum(r => r.NewUsers),
                        PageViews = g.Sum(r => r.PageViews),
                        UniqueVisitors = g.Sum(r => r.UniqueVisitors),
                        NewReviews = g.Sum(r => r.NewReviews),
                        NewTickets = g.Sum(r => r.NewTickets),
                    })
                    .OrderBy(x => x.Date),

                "weekly" => rows
                    .GroupBy(r => IsoWeekKey(r.Date))
                    .Select(g => new DailyStatDto
                    {
                        Date = g.Min(r => r.Date),
                        DateLabel = $"W{g.Key.week}/{g.Key.year}",
                        TotalOrders = g.Sum(r => r.TotalOrders),
                        PaidOrders = g.Sum(r => r.PaidOrders),
                        CancelledOrders = g.Sum(r => r.CancelledOrders),
                        TotalRevenue = g.Sum(r => r.TotalRevenue),
                        NewUsers = g.Sum(r => r.NewUsers),
                        PageViews = g.Sum(r => r.PageViews),
                        UniqueVisitors = g.Sum(r => r.UniqueVisitors),
                        NewReviews = g.Sum(r => r.NewReviews),
                        NewTickets = g.Sum(r => r.NewTickets),
                    })
                    .OrderBy(x => x.Date),

                _ => rows.Select(r => new DailyStatDto
                {
                    Date = r.Date,
                    DateLabel = r.Date.ToString("dd/MM"),
                    TotalOrders = r.TotalOrders,
                    PaidOrders = r.PaidOrders,
                    CancelledOrders = r.CancelledOrders,
                    TotalRevenue = r.TotalRevenue,
                    NewUsers = r.NewUsers,
                    PageViews = r.PageViews,
                    UniqueVisitors = r.UniqueVisitors,
                    NewReviews = r.NewReviews,
                    NewTickets = r.NewTickets,
                })
            };

            var list = items.ToList();

            return ApiResponse<DailyStatsResultDto>.Ok(new DailyStatsResultDto
            {
                Items = list,
                Period = period,
                From = from,
                To = to,
                TotalRevenue = list.Sum(x => x.TotalRevenue),
                TotalOrders = list.Sum(x => x.TotalOrders),
                PaidOrders = list.Sum(x => x.PaidOrders),
                NewUsers = list.Sum(x => x.NewUsers),
                PageViews = list.Sum(x => x.PageViews),
            });
        }

        private static (int year, int week) IsoWeekKey(DateTime date)
        {
            var day = (int)date.DayOfWeek;
            var mon = date.AddDays(day == 0 ? -6 : 1 - day);
            return (mon.Year, System.Globalization.ISOWeek.GetWeekOfYear(mon));
        }
    }
}
