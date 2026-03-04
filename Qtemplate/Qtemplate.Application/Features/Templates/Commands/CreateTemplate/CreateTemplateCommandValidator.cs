using FluentValidation;
using Qtemplate.Application.DTOs.Template.Admin;

namespace Qtemplate.Application.Features.Templates.Commands.CreateTemplate;

public class CreateTemplateCommandValidator : AbstractValidator<CreateTemplateCommand>
{
    public CreateTemplateCommandValidator()
    {
        RuleFor(x => x.Dto.CategoryId)
            .GreaterThan(0).WithMessage("Vui lòng chọn category");

        RuleFor(x => x.Dto.Name)
            .NotEmpty().WithMessage("Tên template không được để trống")
            .MaximumLength(200).WithMessage("Tên không được vượt quá 200 ký tự");

        RuleFor(x => x.Dto.Slug)
            .NotEmpty().WithMessage("Slug không được để trống")
            .MaximumLength(200).WithMessage("Slug không được vượt quá 200 ký tự")
            .Matches("^[a-z0-9-]+$").WithMessage("Slug chỉ được chứa chữ thường, số và dấu gạch ngang");

        RuleFor(x => x.Dto.Price)
            .GreaterThanOrEqualTo(0).WithMessage("Giá không được âm");

        // Nếu free thì giá phải = 0
        RuleFor(x => x.Dto.Price)
            .Equal(0).WithMessage("Template miễn phí phải có giá = 0")
            .When(x => x.Dto.IsFree);

        // Nếu không free thì giá phải > 0
        RuleFor(x => x.Dto.Price)
            .GreaterThan(0).WithMessage("Template trả phí phải có giá > 0")
            .When(x => !x.Dto.IsFree);

        RuleFor(x => x.Dto.Version)
            .Matches(@"^\d+\.\d+\.\d+$").WithMessage("Version phải theo định dạng x.y.z (ví dụ: 1.0.0)")
            .When(x => !string.IsNullOrEmpty(x.Dto.Version));
    }
}