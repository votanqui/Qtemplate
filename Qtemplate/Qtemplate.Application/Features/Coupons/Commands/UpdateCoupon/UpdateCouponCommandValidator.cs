using FluentValidation;
using Qtemplate.Application.Features.Coupons.Commands.UpdateCoupon;

namespace Qtemplate.Application.Features.Coupons.Commands.UpdateCoupon;

public class UpdateCouponCommandValidator : AbstractValidator<UpdateCouponCommand>
{
    public UpdateCouponCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0).WithMessage("ID coupon không hợp lệ");

        RuleFor(x => x.Dto).NotNull().WithMessage("Dữ liệu không được để trống");

        RuleFor(x => x.Dto.Value)
            .GreaterThan(0).WithMessage("Giá trị coupon phải lớn hơn 0");

        RuleFor(x => x.Dto.MinOrderAmount)
            .GreaterThanOrEqualTo(0).WithMessage("Giá trị đơn hàng tối thiểu không được âm")
            .When(x => x.Dto.MinOrderAmount.HasValue);

        RuleFor(x => x.Dto.MaxDiscountAmount)
            .GreaterThan(0).WithMessage("Giảm giá tối đa phải lớn hơn 0")
            .When(x => x.Dto.MaxDiscountAmount.HasValue);

        RuleFor(x => x.Dto.UsageLimit)
            .GreaterThan(0).WithMessage("Giới hạn sử dụng phải lớn hơn 0")
            .When(x => x.Dto.UsageLimit.HasValue);

        RuleFor(x => x.Dto.ExpiredAt)
            .GreaterThan(DateTime.UtcNow).WithMessage("Ngày hết hạn phải ở tương lai")
            .When(x => x.Dto.ExpiredAt.HasValue);

        RuleFor(x => x.Dto.ExpiredAt)
            .GreaterThan(x => x.Dto.StartAt).WithMessage("Ngày hết hạn phải sau ngày bắt đầu")
            .When(x => x.Dto.StartAt.HasValue && x.Dto.ExpiredAt.HasValue);
    }
}