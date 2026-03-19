using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qtemplate.Application.Features.Community.Commands.UpdateComment
{
    public class UpdateCommentCommandValidator : AbstractValidator<UpdateCommentCommand>
    {
        public UpdateCommentCommandValidator()
        {
            RuleFor(x => x.Content)
                .NotEmpty()
                .WithMessage("Nội dung không được để trống")
                .MaximumLength(1000)
                .WithMessage("Bình luận không được vượt quá 1000 ký tự");

            RuleFor(x => x.CommentId)
                .GreaterThan(0)
                .WithMessage("CommentId không hợp lệ");

            RuleFor(x => x.UserId)
                .NotEqual(Guid.Empty)
                .WithMessage("UserId không hợp lệ");
        }
    }
}
