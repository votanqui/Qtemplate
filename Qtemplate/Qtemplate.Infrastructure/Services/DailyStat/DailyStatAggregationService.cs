using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Qtemplate.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qtemplate.Infrastructure.Services.DailyStat
{
    public class DailyStatAggregationService : BackgroundService
    {
        private readonly IServiceProvider _services;
        private readonly ILogger<DailyStatAggregationService> _logger;

        public DailyStatAggregationService(
            IServiceProvider services,
            ILogger<DailyStatAggregationService> logger)
        {
            _services = services;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("DailyStatAggregationService started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var delay = ComputeDelayUntilMidnightUtc();
                    _logger.LogInformation(
                        "DailyStatAggregationService: next run in {Minutes:F0} minutes.", delay.TotalMinutes);

                    await Task.Delay(delay, stoppingToken);

                    await AggregateYesterdayAsync();
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "DailyStatAggregationService error.");
                    // Nếu lỗi, chờ 10 phút rồi thử lại tránh vòng lặp nhanh
                    await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);
                }
            }

            _logger.LogInformation("DailyStatAggregationService stopped.");
        }

        // ─────────────────────────────────────────────────────────────────────────
        // Tính delay đến 00:00:10 UTC ngày mai (10 giây sau nửa đêm để DB settle)
        // ─────────────────────────────────────────────────────────────────────────
        private static TimeSpan ComputeDelayUntilMidnightUtc()
        {
            var now = DateTime.UtcNow;
            var midnight = now.Date.AddDays(1).AddSeconds(10); // 00:00:10 UTC ngày mai
            return midnight - now;
        }

        // ─────────────────────────────────────────────────────────────────────────
        private async Task AggregateYesterdayAsync()
        {
            // Tổng hợp ngày hôm qua (UTC)
            var yesterday = DateTime.UtcNow.Date.AddDays(-1);
            await AggregateForDateAsync(yesterday);
        }

        // ─────────────────────────────────────────────────────────────────────────
        // Public để có thể gọi thủ công (backfill) từ admin endpoint nếu cần
        // ─────────────────────────────────────────────────────────────────────────
        public async Task AggregateForDateAsync(DateTime date)
        {
            using var scope = _services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var dayStart = date.Date;
            var dayEnd = dayStart.AddDays(1);

            // ── Orders ────────────────────────────────────────────────────────────
            var orders = await db.Orders
                .Where(o => o.CreatedAt >= dayStart && o.CreatedAt < dayEnd)
                .Select(o => new { o.Status, o.FinalAmount })
                .ToListAsync();

            int totalOrders = orders.Count;
            int paidOrders = orders.Count(o => o.Status is "Paid" or "Completed");
            int cancelledOrders = orders.Count(o => o.Status == "Cancelled");
            decimal revenue = orders
                .Where(o => o.Status is "Paid" or "Completed")
                .Sum(o => o.FinalAmount);

            // ── New users ─────────────────────────────────────────────────────────
            int newUsers = await db.Users
                .Where(u => u.CreatedAt >= dayStart && u.CreatedAt < dayEnd)
                .CountAsync();

            // ── Analytics (pageviews / unique visitors) ───────────────────────────
            var analyticsRows = await db.Analytics
                .Where(a => a.CreatedAt >= dayStart && a.CreatedAt < dayEnd)
                .Select(a => a.SessionId)
                .ToListAsync();

            int pageViews = analyticsRows.Count;
            int uniqueVisitors = analyticsRows
                .Where(s => s != null)
                .Distinct()
                .Count();

            // ── Reviews ───────────────────────────────────────────────────────────
            int newReviews = await db.Reviews
                .Where(r => r.CreatedAt >= dayStart && r.CreatedAt < dayEnd)
                .CountAsync();

            // ── Support Tickets ───────────────────────────────────────────────────
            int newTickets = await db.SupportTickets
                .Where(t => t.CreatedAt >= dayStart && t.CreatedAt < dayEnd)
                .CountAsync();

            // ── Upsert vào DailyStats (idempotent) ───────────────────────────────
            var existing = await db.DailyStats
                .FirstOrDefaultAsync(s => s.Date == dayStart);

            if (existing is null)
            {
                await db.DailyStats.AddAsync(new Domain.Entities.DailyStat
                {
                    Date = dayStart,
                    TotalOrders = totalOrders,
                    PaidOrders = paidOrders,
                    CancelledOrders = cancelledOrders,
                    TotalRevenue = revenue,
                    NewUsers = newUsers,
                    PageViews = pageViews,
                    UniqueVisitors = uniqueVisitors,
                    NewReviews = newReviews,
                    NewTickets = newTickets,
                    CreatedAt = DateTime.UtcNow
                });
            }
            else
            {
                existing.TotalOrders = totalOrders;
                existing.PaidOrders = paidOrders;
                existing.CancelledOrders = cancelledOrders;
                existing.TotalRevenue = revenue;
                existing.NewUsers = newUsers;
                existing.PageViews = pageViews;
                existing.UniqueVisitors = uniqueVisitors;
                existing.NewReviews = newReviews;
                existing.NewTickets = newTickets;
            }

            await db.SaveChangesAsync();

            _logger.LogInformation(
                "DailyStatAggregationService: aggregated {Date:yyyy-MM-dd} — " +
                "Orders={Total}(Paid={Paid},Cancel={Cancel}), Revenue={Rev:N0}, " +
                "Users={Users}, PageViews={PV}, UniqueVisitors={UV}, Reviews={Rev2}, Tickets={Tick}",
                dayStart, totalOrders, paidOrders, cancelledOrders, revenue,
                newUsers, pageViews, uniqueVisitors, newReviews, newTickets);
        }
    }
}
