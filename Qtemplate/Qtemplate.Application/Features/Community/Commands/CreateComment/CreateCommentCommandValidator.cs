using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qtemplate.Application.Features.Community.Commands.CreateComment
{
    public class CreateCommentCommandValidator : AbstractValidator<CreateCommentCommand>
    {
        public CreateCommentCommandValidator()
        {
            RuleFor(x => x.Content)
                .NotEmpty()
                .WithMessage("Nội dung bình luận không được để trống")
                .MaximumLength(1000)
                .WithMessage("Bình luận không được vượt quá 1000 ký tự");

            RuleFor(x => x.PostId)
                .GreaterThan(0)
                .WithMessage("PostId không hợp lệ");

            RuleFor(x => x.UserId)
                .NotEqual(Guid.Empty)
                .WithMessage("UserId không hợp lệ");
        }
    }
}
