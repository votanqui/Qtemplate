using FluentValidation;

namespace Qtemplate.Application.Features.UserManagement.Commands.UpdateProfile;

public class UpdateProfileCommandValidator : AbstractValidator<UpdateProfileCommand>
{
    public UpdateProfileCommandValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Họ tên không được để trống")
            .MaximumLength(100).WithMessage("Họ tên không được vượt quá 100 ký tự");

        RuleFor(x => x.PhoneNumber)
            .Matches(@"^(0|\+84)\d{9}$").WithMessage("Số điện thoại không đúng định dạng")
            .When(x => !string.IsNullOrEmpty(x.PhoneNumber));
    }
}