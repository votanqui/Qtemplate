// Qtemplate.Application/Features/UserManagement/Commands/UpdateAvatar/UpdateAvatarCommandValidator.cs
using FluentValidation;

namespace Qtemplate.Application.Features.UserManagement.Commands.UpdateAvatar;

public class UpdateAvatarCommandValidator : AbstractValidator<UpdateAvatarCommand>
{
    private static readonly string[] AllowedExtensions = [".jpg", ".jpeg", ".png", ".webp"];
    private const long MaxBytes = 2 * 1024 * 1024; // 2MB

    public UpdateAvatarCommandValidator()
    {
        RuleFor(x => x.File)
            .NotNull().WithMessage("Vui lòng chọn file ảnh");

        When(x => x.File is not null, () =>
        {
            RuleFor(x => x.File.Length)
                .GreaterThan(0).WithMessage("File không được rỗng")
                .LessThanOrEqualTo(MaxBytes).WithMessage("File không được vượt quá 2MB");

            RuleFor(x => x.File.FileName)
                .Must(name =>
                {
                    var ext = Path.GetExtension(name).ToLower();
                    return AllowedExtensions.Contains(ext);
                })
                .WithMessage("Chỉ chấp nhận file JPG, PNG, WEBP");

            // Chặn double extension: avatar.php.jpg
            RuleFor(x => x.File.FileName)
                .Must(name => Path.GetFileNameWithoutExtension(name).All(c => c != '.'))
                .WithMessage("Tên file không hợp lệ");

            RuleFor(x => x.File.ContentType)
                .Must(ct => ct is "image/jpeg" or "image/png" or "image/webp")
                .WithMessage("Content-Type không hợp lệ");
        });
    }
}