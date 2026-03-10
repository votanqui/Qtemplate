using FluentValidation;
using Qtemplate.Application.Features.Auth.Commands.ForgotPassword;

namespace Qtemplate.Application.Features.Auth.Commands.ForgotPassword;

public class ForgotPasswordCommandValidator : AbstractValidator<ForgotPasswordCommand>
{
    public ForgotPasswordCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email không được để trống")
            .EmailAddress().WithMessage("Email không đúng định dạng")
            .MaximumLength(256).WithMessage("Email không được vượt quá 256 ký tự");
    }
}