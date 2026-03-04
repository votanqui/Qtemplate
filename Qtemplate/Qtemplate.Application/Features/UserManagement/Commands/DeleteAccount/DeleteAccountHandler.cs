using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.Services.Interfaces;
using Qtemplate.Domain.Interfaces.Repositories;

namespace Qtemplate.Application.Features.UserManagement.Commands.DeleteAccount;

public class DeleteAccountHandler : IRequestHandler<DeleteAccountCommand, ApiResponse<bool>>
{
    private readonly IUserRepository _userRepo;
    private readonly IRefreshTokenRepository _refreshTokenRepo;
    private readonly IAuditLogService _auditLogService;

    public DeleteAccountHandler(
        IUserRepository userRepo,
        IRefreshTokenRepository refreshTokenRepo,
        IAuditLogService auditLogService)
    {
        _userRepo = userRepo;
        _refreshTokenRepo = refreshTokenRepo;
        _auditLogService = auditLogService;
    }

    public async Task<ApiResponse<bool>> Handle(DeleteAccountCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepo.GetByIdAsync(request.UserId);
        if (user is null)
            return ApiResponse<bool>.Fail("Không tìm thấy người dùng");

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return ApiResponse<bool>.Fail("Mật khẩu không chính xác");

        // Soft delete: deactivate account
        user.IsActive = false;
        user.UpdatedAt = DateTime.UtcNow;
        await _userRepo.UpdateAsync(user);

        // Revoke tất cả refresh tokens
        await _refreshTokenRepo.RevokeAllByUserIdAsync(user.Id, "AccountDeleted");

        await _auditLogService.LogAsync(
            userId: user.Id.ToString(),
            userEmail: user.Email,
            action: "DeleteAccount",
            entityName: "User",
            entityId: user.Id.ToString(),
            ipAddress: request.IpAddress
        );

        return ApiResponse<bool>.Ok(true, "Tài khoản đã được xóa thành công");
    }
}