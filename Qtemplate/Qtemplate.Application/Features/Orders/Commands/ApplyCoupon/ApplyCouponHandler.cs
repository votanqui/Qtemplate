using MediatR;
using Qtemplate.Application.DTOs.Order;
using Qtemplate.Application.DTOs;
using Qtemplate.Domain.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qtemplate.Application.Features.Orders.Commands.ApplyCoupon
{
    public class ApplyCouponHandler : IRequestHandler<ApplyCouponQuery, ApiResponse<ApplyCouponResultDto>>
    {
        private readonly ICouponRepository _couponRepo;
        private readonly ITemplateRepository _templateRepo;

        public ApplyCouponHandler(ICouponRepository couponRepo, ITemplateRepository templateRepo)
        {
            _couponRepo = couponRepo;
            _templateRepo = templateRepo;
        }

        public async Task<ApiResponse<ApplyCouponResultDto>> Handle(
     ApplyCouponQuery request, CancellationToken cancellationToken)
        {
            // Validate input
            if (!request.TemplateIds.Any())
                return ApiResponse<ApplyCouponResultDto>.Fail("Vui lòng chọn template trước khi áp mã");

            decimal total = 0;
            foreach (var tid in request.TemplateIds)
            {
                var t = await _templateRepo.GetByIdAsync(tid);
                if (t is null || t.Status != "Published")
                    return ApiResponse<ApplyCouponResultDto>.Fail($"Template {tid} không tồn tại");
                total += t.SalePrice ?? t.Price;
            }

            var coupon = await _couponRepo.GetByCodeAsync(request.CouponCode);
            if (coupon is null || !coupon.IsActive)
                return ApiResponse<ApplyCouponResultDto>.Fail("Mã giảm giá không hợp lệ");

            if (coupon.StartAt.HasValue && DateTime.UtcNow < coupon.StartAt)
                return ApiResponse<ApplyCouponResultDto>.Fail("Mã chưa có hiệu lực");

            if (coupon.ExpiredAt.HasValue && DateTime.UtcNow > coupon.ExpiredAt)
                return ApiResponse<ApplyCouponResultDto>.Fail("Mã đã hết hạn");

            if (coupon.UsageLimit.HasValue && coupon.UsedCount >= coupon.UsageLimit)
                return ApiResponse<ApplyCouponResultDto>.Fail("Mã đã hết lượt dùng");

            if (coupon.MinOrderAmount.HasValue && total < coupon.MinOrderAmount)
                return ApiResponse<ApplyCouponResultDto>.Fail(
                    $"Đơn tối thiểu {coupon.MinOrderAmount:N0}đ (hiện tại: {total:N0}đ)");

            var discount = coupon.Type == "Percent"
                ? total * coupon.Value / 100
                : coupon.Value;

            if (coupon.MaxDiscountAmount.HasValue)
                discount = Math.Min(discount, coupon.MaxDiscountAmount.Value);

            discount = Math.Min(discount, total);

            return ApiResponse<ApplyCouponResultDto>.Ok(new ApplyCouponResultDto
            {
                CouponCode = coupon.Code,
                Type = coupon.Type,
                Value = coupon.Value,
                TotalAmount = total,
                DiscountAmount = discount,
                FinalAmount = total - discount
            }, $"Áp dụng thành công, giảm {discount:N0}đ");
        }
    }
}
