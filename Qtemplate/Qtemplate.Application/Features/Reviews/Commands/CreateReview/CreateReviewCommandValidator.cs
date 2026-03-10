using FluentValidation;
using Qtemplate.Application.Features.Reviews.Commands.CreateReview;

namespace Qtemplate.Application.Features.Reviews.Commands.CreateReview;

public class CreateReviewCommandValidator : AbstractValidator<CreateReviewCommand>
{
    public CreateReviewCommandValidator()
    {
        RuleFor(x => x.Slug)
            .NotEmpty().WithMessage("Slug template không được để trống");

        RuleFor(x => x.Rating)
            .InclusiveBetween(1, 5).WithMessage("Đánh giá phải từ 1 đến 5 sao");

        RuleFor(x => x.Title)
            .MaximumLength(200).WithMessage("Tiêu đề không được vượt quá 200 ký tự")
            .When(x => x.Title != null);

        RuleFor(x => x.Comment)
            .MaximumLength(2000).WithMessage("Nội dung đánh giá không được vượt quá 2000 ký tự")
            .When(x => x.Comment != null);
    }
}