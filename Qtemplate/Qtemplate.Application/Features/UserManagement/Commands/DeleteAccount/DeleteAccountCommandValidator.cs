using FluentValidation;

namespace Qtemplate.Application.Features.UserManagement.Commands.DeleteAccount;

public class DeleteAccountCommandValidator : AbstractValidator<DeleteAccountCommand>
{
    public DeleteAccountCommandValidator()
    {
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Vui lòng nhập mật khẩu để xác nhận xóa tài khoản");
    }
}