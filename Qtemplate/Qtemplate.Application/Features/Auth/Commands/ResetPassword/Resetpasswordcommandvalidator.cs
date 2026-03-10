using FluentValidation;
using Qtemplate.Application.Features.Auth.Commands.ResetPassword;

namespace Qtemplate.Application.Features.Auth.Commands.ResetPassword;

public class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordCommandValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Token không được để trống");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("Mật khẩu mới không được để trống")
            .MinimumLength(6).WithMessage("Mật khẩu mới phải có ít nhất 6 ký tự")
            .MaximumLength(100).WithMessage("Mật khẩu mới không được vượt quá 100 ký tự");

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty().WithMessage("Xác nhận mật khẩu không được để trống")
            .Equal(x => x.NewPassword).WithMessage("Xác nhận mật khẩu không khớp");
    }
}