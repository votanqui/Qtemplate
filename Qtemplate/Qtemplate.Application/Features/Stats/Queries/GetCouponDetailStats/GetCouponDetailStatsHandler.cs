using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.DTOs.Stats;
using Qtemplate.Domain.Interfaces.Repositories;

namespace Qtemplate.Application.Features.Stats.Queries.GetCouponDetailStats;

public class GetCouponDetailStatsHandler
    : IRequestHandler<GetCouponDetailStatsQuery, ApiResponse<CouponDetailStatsDto>>
{
    private readonly IStatsRepository _stats;
    public GetCouponDetailStatsHandler(IStatsRepository stats) => _stats = stats;

    public async Task<ApiResponse<CouponDetailStatsDto>> Handle(
        GetCouponDetailStatsQuery request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var in7Days = now.AddDays(7);

        var coupons = await _stats.GetAllCouponsAsync();
        var paidOrders = await _stats.GetPaidOrdersAsync();
        var couponUsage = await _stats.GetCouponUsageAsync();

        var withCoupon = paidOrders.Count(o => !string.IsNullOrEmpty(o.CouponCode));
        var totalPaid = paidOrders.Count;
        var withDiscount = paidOrders.Where(o => o.DiscountAmount > 0).ToList();

        return ApiResponse<CouponDetailStatsDto>.Ok(new CouponDetailStatsDto
        {
            TotalCoupons = coupons.Count,
            ActiveCoupons = coupons.Count(c => c.IsActive && (c.ExpiredAt == null || c.ExpiredAt > now)),
            ExpiredCoupons = coupons.Count(c => c.ExpiredAt.HasValue && c.ExpiredAt <= now),
            TotalDiscounted = couponUsage.Sum(u => u.TotalDiscount),
            OrdersWithCoupon = withCoupon,
            OrdersWithoutCoupon = totalPaid - withCoupon,
            CouponUsageRate = totalPaid == 0 ? 0
                : Math.Round((decimal)withCoupon / totalPaid * 100, 2),
            AverageDiscount = withDiscount.Count == 0 ? 0
                : Math.Round(withDiscount.Sum(o => o.DiscountAmount) / withDiscount.Count, 0),
            TopCoupons = coupons
                .Select(c => new TopCouponDto
                {
                    Code = c.Code,
                    Type = c.Type,
                    Value = c.Value,
                    UsedCount = c.UsedCount,
                    TotalDiscount = couponUsage.FirstOrDefault(u => u.Code == c.Code).TotalDiscount
                })
                .OrderByDescending(x => x.UsedCount)
                .Take(10).ToList(),
            ExpiringSoon = coupons
                .Where(c => c.IsActive && c.ExpiredAt.HasValue
                         && c.ExpiredAt > now && c.ExpiredAt <= in7Days)
                .Select(c => new ExpiringSoonDto
                {
                    Id = c.Id,
                    Code = c.Code,
                    Type = c.Type,
                    Value = c.Value,
                    ExpiredAt = c.ExpiredAt!.Value,
                    DaysLeft = (int)(c.ExpiredAt.Value - now).TotalDays,
                    UsedCount = c.UsedCount,
                    UsageLimit = c.UsageLimit
                })
                .OrderBy(x => x.DaysLeft).ToList(),
            LowUsage = coupons
                .Where(c => c.IsActive && c.UsageLimit.HasValue
                         && (c.UsageLimit.Value - c.UsedCount) is > 0 and <= 5)
                .Select(c => new LowUsageDto
                {
                    Id = c.Id,
                    Code = c.Code,
                    UsedCount = c.UsedCount,
                    UsageLimit = c.UsageLimit!.Value,
                    RemainingUse = c.UsageLimit.Value - c.UsedCount
                })
                .OrderBy(x => x.RemainingUse).ToList()
        });
    }
}