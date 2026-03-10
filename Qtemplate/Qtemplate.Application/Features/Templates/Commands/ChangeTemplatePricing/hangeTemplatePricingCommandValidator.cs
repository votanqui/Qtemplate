using FluentValidation;
using Qtemplate.Application.Features.Templates.Commands.ChangeTemplatePricing;

namespace Qtemplate.Application.Features.Templates.Commands.ChangeTemplatePricing;

public class ChangeTemplatePricingCommandValidator : AbstractValidator<ChangeTemplatePricingCommand>
{
    public ChangeTemplatePricingCommandValidator()
    {
        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Giá phải lớn hơn 0")
            .When(x => !x.IsFree);

        RuleFor(x => x.Price)
            .Equal(0).WithMessage("Template miễn phí phải có giá bằng 0")
            .When(x => x.IsFree);
    }
}