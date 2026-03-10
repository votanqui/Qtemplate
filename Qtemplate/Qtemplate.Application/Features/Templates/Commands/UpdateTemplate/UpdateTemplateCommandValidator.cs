using FluentValidation;
using Qtemplate.Application.Features.Templates.Commands.UpdateTemplate;

namespace Qtemplate.Application.Features.Templates.Commands.UpdateTemplate;

public class UpdateTemplateCommandValidator : AbstractValidator<UpdateTemplateCommand>
{
    public UpdateTemplateCommandValidator()
    {
        RuleFor(x => x.Dto).NotNull().WithMessage("Dữ liệu cập nhật không được để trống");

        RuleFor(x => x.Dto.Name)
            .NotEmpty().WithMessage("Tên template không được để trống")
            .MaximumLength(200).WithMessage("Tên template không được vượt quá 200 ký tự");

        RuleFor(x => x.Dto.Slug)
            .NotEmpty().WithMessage("Slug không được để trống")
            .MaximumLength(200).WithMessage("Slug không được vượt quá 200 ký tự")
            .Matches(@"^[a-z0-9]+(?:-[a-z0-9]+)*$").WithMessage("Slug chỉ được chứa chữ thường, số và dấu gạch ngang");

        RuleFor(x => x.Dto.CategoryId)
            .GreaterThan(0).WithMessage("Danh mục không hợp lệ");

        RuleFor(x => x.Dto.Price)
            .GreaterThanOrEqualTo(0).WithMessage("Giá không được âm")
            .When(x => !x.Dto.IsFree);

        RuleFor(x => x.Dto.SalePrice)
            .GreaterThan(0).WithMessage("Giá khuyến mãi phải lớn hơn 0")
            .LessThan(x => x.Dto.Price).WithMessage("Giá khuyến mãi phải nhỏ hơn giá gốc")
            .When(x => x.Dto.SalePrice.HasValue && !x.Dto.IsFree);

        RuleFor(x => x.Dto.SaleEndAt)
            .GreaterThan(x => x.Dto.SaleStartAt).WithMessage("Ngày kết thúc khuyến mãi phải sau ngày bắt đầu")
            .When(x => x.Dto.SaleStartAt.HasValue && x.Dto.SaleEndAt.HasValue);

        RuleFor(x => x.Dto.ShortDescription)
            .MaximumLength(500).WithMessage("Mô tả ngắn không được vượt quá 500 ký tự")
            .When(x => x.Dto.ShortDescription != null);
    }
}