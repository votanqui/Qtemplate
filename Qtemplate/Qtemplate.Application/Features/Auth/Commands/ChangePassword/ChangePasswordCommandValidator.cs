using FluentValidation;
namespace Qtemplate.Application.Features.Auth.Commands.ChangePassword;

public class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordCommandValidator()
    {
        RuleFor(x => x.CurrentPassword)
            .NotEmpty().WithMessage("Mật khẩu hiện tại không được để trống");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("Mật khẩu mới không được để trống")
            .MinimumLength(6).WithMessage("Mật khẩu mới phải có ít nhất 6 ký tự")
            .MaximumLength(100).WithMessage("Mật khẩu không được vượt quá 100 ký tự")
            .NotEqual(x => x.CurrentPassword).WithMessage("Mật khẩu mới không được trùng mật khẩu cũ");

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty().WithMessage("Xác nhận mật khẩu không được để trống")
            .Equal(x => x.NewPassword).WithMessage("Mật khẩu xác nhận không khớp");
    }
}