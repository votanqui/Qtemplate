using MediatR;
using Microsoft.Extensions.Configuration;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.Services;
using Qtemplate.Application.Services.Interfaces;
using Qtemplate.Domain.Interfaces.Repositories;

namespace Qtemplate.Application.Features.Auth.Commands.ResendVerifyEmail;

public class ResendVerifyEmailHandler : IRequestHandler<ResendVerifyEmailCommand, ApiResponse<object>>
{
    private readonly IUserRepository _userRepo;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _config;

    public ResendVerifyEmailHandler(
        IUserRepository userRepo,
        IEmailService emailService,
        IConfiguration config)
    {
        _userRepo = userRepo;
        _emailService = emailService;
        _config = config;
    }

    public async Task<ApiResponse<object>> Handle(ResendVerifyEmailCommand request, CancellationToken cancellationToken)
    {
        var email = request.Email.Trim().ToLower();
        var user = await _userRepo.GetByEmailAsync(email);

        // Không lộ thông tin email có tồn tại hay không
        if (user is null)
            return ApiResponse<object>.Ok(null!, "Nếu email tồn tại và chưa xác thực, chúng tôi đã gửi lại email xác thực");

        // Tách riêng trường hợp đã verify → thông báo rõ
        if (user.IsEmailVerified)
            return ApiResponse<object>.Fail("Email này đã được xác thực, vui lòng đăng nhập");

        user.EmailVerifyToken = Guid.NewGuid().ToString("N");
        user.UpdatedAt = DateTime.UtcNow;
        await _userRepo.UpdateAsync(user);

        var verifyUrl = $"{_config["App:BaseUrl"]}/api/auth/verifyemail?token={user.EmailVerifyToken}";

        try
        {
            await _emailService.SendDirectAsync(
         user.Email,
         "Xác minh tài khoản của bạn",
         EmailTemplates.VerifyEmail(user.FullName, verifyUrl)
     );
        }
        catch
        {
            return ApiResponse<object>.Fail("Gửi email thất bại, vui lòng thử lại sau");
        }

        return ApiResponse<object>.Ok(null!, "Email xác thực đã được gửi lại, vui lòng kiểm tra hộp thư");
    }
}