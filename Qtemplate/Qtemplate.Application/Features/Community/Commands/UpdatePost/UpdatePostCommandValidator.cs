using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qtemplate.Application.Features.Community.Commands.UpdatePost
{
    public class UpdatePostCommandValidator : AbstractValidator<UpdatePostCommand>
    {
        public UpdatePostCommandValidator()
        {
            RuleFor(x => x)
                .Must(x => !string.IsNullOrWhiteSpace(x.Content) || !string.IsNullOrWhiteSpace(x.ImageUrl))
                .WithMessage("Bài viết phải có nội dung hoặc ảnh");

            RuleFor(x => x.Content)
                .MaximumLength(3000)
                .WithMessage("Nội dung không được vượt quá 3000 ký tự")
                .When(x => !string.IsNullOrWhiteSpace(x.Content));

            RuleFor(x => x.PostId)
                .GreaterThan(0)
                .WithMessage("PostId không hợp lệ");

            RuleFor(x => x.UserId)
                .NotEqual(Guid.Empty)
                .WithMessage("UserId không hợp lệ");
        }
    }
}
