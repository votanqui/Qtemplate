using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.DTOs.Stats;
using Qtemplate.Domain.Interfaces.Repositories;

namespace Qtemplate.Application.Features.Stats.Queries.GetDashboardStats;

public class GetDashboardStatsHandler
    : IRequestHandler<GetDashboardStatsQuery, ApiResponse<DashboardStatsDto>>
{
    private readonly IStatsRepository _stats;
    public GetDashboardStatsHandler(IStatsRepository stats) => _stats = stats;

    public async Task<ApiResponse<DashboardStatsDto>> Handle(
        GetDashboardStatsQuery request, CancellationToken cancellationToken)
    {
        var to = (request.To ?? DateTime.UtcNow).Date.AddDays(1).AddSeconds(-1);
        var from = (request.From ?? to.AddDays(-29)).Date;
        var now = DateTime.UtcNow;

        // Tất cả query đều có date range — không load toàn bảng
        var orders = await _stats.GetOrdersInRangeAsync(from, to, includeItems: true);
        var payments = await _stats.GetPaymentsInRangeAsync(from, to);
        var coupons = await _stats.GetAllCouponsAsync();
        var couponUsage = await _stats.GetCouponUsageAsync();

        var paid = orders.Where(o => o.Status == "Paid").ToList();

        // ── Order stats ──
        var topTemplates = paid
            .SelectMany(o => o.Items)
            .GroupBy(i => new { i.TemplateId, i.TemplateName })
            .Select(g => new TopTemplateDto
            {
                TemplateId = g.Key.TemplateId,
                TemplateName = g.Key.TemplateName,
                SalesCount = g.Count(),
                Revenue = g.Sum(i => i.Price)
            })
            .OrderByDescending(x => x.SalesCount)
            .Take(10).ToList();

        var topUsers = paid
            .GroupBy(o => new { o.UserId, o.User.Email, o.User.FullName })
            .Select(g => new TopUserDto
            {
                UserId = g.Key.UserId,
                Email = g.Key.Email,
                FullName = g.Key.FullName,
                OrderCount = g.Count(),
                TotalSpent = g.Sum(o => o.FinalAmount)
            })
            .OrderByDescending(x => x.TotalSpent)
            .Take(10).ToList();

        // ── Coupon stats ──
        var topCoupons = coupons
            .Select(c => new TopCouponDto
            {
                Code = c.Code,
                Type = c.Type,
                Value = c.Value,
                UsedCount = c.UsedCount,
                TotalDiscount = couponUsage.FirstOrDefault(u => u.Code == c.Code).TotalDiscount
            })
            .OrderByDescending(x => x.UsedCount)
            .Take(10).ToList();

        return ApiResponse<DashboardStatsDto>.Ok(new DashboardStatsDto
        {
            Orders = new OrderStatsDto
            {
                TotalOrders = orders.Count,
                PendingOrders = orders.Count(o => o.Status == "Pending"),
                PaidOrders = paid.Count,
                CancelledOrders = orders.Count(o => o.Status == "Cancelled"),
                TotalRevenue = paid.Sum(o => o.FinalAmount),
                TotalDiscount = paid.Sum(o => o.DiscountAmount),
                RevenueByDay = paid
                    .GroupBy(o => o.CreatedAt.ToString("yyyy-MM-dd"))
                    .Select(g => new RevenueByPeriodDto { Label = g.Key, Revenue = g.Sum(o => o.FinalAmount), Orders = g.Count() })
                    .OrderBy(x => x.Label).ToList(),
                RevenueByMonth = paid
                    .GroupBy(o => o.CreatedAt.ToString("yyyy-MM"))
                    .Select(g => new RevenueByPeriodDto { Label = g.Key, Revenue = g.Sum(o => o.FinalAmount), Orders = g.Count() })
                    .OrderBy(x => x.Label).ToList(),
                RevenueByYear = paid
                    .GroupBy(o => o.CreatedAt.ToString("yyyy"))
                    .Select(g => new RevenueByPeriodDto { Label = g.Key, Revenue = g.Sum(o => o.FinalAmount), Orders = g.Count() })
                    .OrderBy(x => x.Label).ToList(),
                TopTemplates = topTemplates,
                TopUsers = topUsers
            },
            Payments = new PaymentStatsDto
            {
                TotalTransactions = payments.Count,
                SuccessTransactions = payments.Count(p => p.Status == "Paid"),
                FailedTransactions = payments.Count(p => p.Status == "Failed"),
                PendingTransactions = payments.Count(p => p.Status == "Pending"),
                TotalPaid = payments.Where(p => p.Status == "Paid").Sum(p => p.Amount),
                ByBank = payments
                    .Where(p => p.Status == "Paid" && !string.IsNullOrEmpty(p.BankCode))
                    .GroupBy(p => p.BankCode!)
                    .Select(g => new RevenueByBankDto { BankCode = g.Key, Count = g.Count(), TotalAmount = g.Sum(p => p.Amount) })
                    .OrderByDescending(x => x.TotalAmount).ToList()
            },
            Coupons = new CouponStatsDto
            {
                TotalCoupons = coupons.Count,
                ActiveCoupons = coupons.Count(c => c.IsActive && (c.ExpiredAt == null || c.ExpiredAt > now)),
                ExpiredCoupons = coupons.Count(c => c.ExpiredAt.HasValue && c.ExpiredAt <= now),
                TotalDiscounted = couponUsage.Sum(u => u.TotalDiscount),
                TopCoupons = topCoupons
            }
        });
    }
}