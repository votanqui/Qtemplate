using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.Services.Interfaces;
using Qtemplate.Domain.Interfaces.Repositories;

namespace Qtemplate.Application.Features.Admin.Users.Commands.ChangeUserRole;

public class ChangeUserRoleHandler
    : IRequestHandler<ChangeUserRoleCommand, ApiResponse<object>>
{
    private readonly IUserRepository _userRepo;
    private readonly IAuditLogService _auditLogService;

    public ChangeUserRoleHandler(IUserRepository userRepo, IAuditLogService auditLogService)
    {
        _userRepo = userRepo;
        _auditLogService = auditLogService;
    }

    public async Task<ApiResponse<object>> Handle(
        ChangeUserRoleCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepo.GetByIdAsync(request.TargetUserId);
        if (user is null)
            return ApiResponse<object>.Fail("Không tìm thấy người dùng");

        var validRoles = new[] { "User", "Admin", "Staff" };
        if (!validRoles.Contains(request.Role))
            return ApiResponse<object>.Fail("Role không hợp lệ. Chỉ chấp nhận: User, Admin, Staff");

        var oldRole = user.Role;
        user.Role = request.Role;
        user.UpdatedAt = DateTime.UtcNow;
        await _userRepo.UpdateAsync(user);

        await _auditLogService.LogAsync(
            userId: request.AdminId,
            userEmail: request.AdminEmail,
            action: "ChangeUserRole",
            entityName: "User",
            entityId: user.Id.ToString(),
            oldValues: new { Role = oldRole },
            newValues: new { Role = request.Role },
            ipAddress: request.IpAddress);

        return ApiResponse<object>.Ok(null!, $"Đã đổi role thành {request.Role}");
    }
}