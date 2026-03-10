using FluentValidation;
using Qtemplate.Application.Features.Coupons.Commands.CreateCoupon;

namespace Qtemplate.Application.Features.Coupons.Commands.CreateCoupon;

public class CreateCouponCommandValidator : AbstractValidator<CreateCouponCommand>
{
    private static readonly string[] ValidTypes = { "Percent", "Fixed" };

    public CreateCouponCommandValidator()
    {
        RuleFor(x => x.Dto).NotNull().WithMessage("Dữ liệu không được để trống");

        RuleFor(x => x.Dto.Code)
            .NotEmpty().WithMessage("Mã coupon không được để trống")
            .MaximumLength(50).WithMessage("Mã coupon không được vượt quá 50 ký tự")
            .Matches(@"^[A-Z0-9_\-]+$").WithMessage("Mã coupon chỉ được chứa chữ hoa, số, dấu gạch ngang và gạch dưới");

        RuleFor(x => x.Dto.Type)
            .NotEmpty().WithMessage("Loại coupon không được để trống")
            .Must(t => ValidTypes.Contains(t)).WithMessage("Loại coupon phải là 'Percent' hoặc 'Fixed'");

        RuleFor(x => x.Dto.Value)
            .GreaterThan(0).WithMessage("Giá trị coupon phải lớn hơn 0");

        RuleFor(x => x.Dto.Value)
            .LessThanOrEqualTo(100).WithMessage("Phần trăm giảm giá không được vượt quá 100%")
            .When(x => x.Dto.Type == "Percent");

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