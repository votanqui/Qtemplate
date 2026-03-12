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

    public ChangeUserRoleHandler(
        IUserRepository userRepo,
        IAuditLogService auditLogService)
    {
        _userRepo = userRepo;
        _auditLogService = auditLogService;
    }

    public async Task<ApiResponse<object>> Handle(
        ChangeUserRoleCommand request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(request.AdminId) || !Guid.TryParse(request.AdminId, out var adminGuid))
            return ApiResponse<object>.Fail("Admin không hợp lệ");

        var admin = await _userRepo.GetByIdAsync(adminGuid);
        if (admin is null)
            return ApiResponse<object>.Fail("Không tìm thấy admin");

        // ❌ chỉ Admin mới được đổi role
        if (admin.Role != "Admin")
            return ApiResponse<object>.Fail("Bạn không có quyền thực hiện hành động này");

        var user = await _userRepo.GetByIdAsync(request.TargetUserId);
        if (user is null)
            return ApiResponse<object>.Fail("Không tìm thấy người dùng");

        // ❌ không cho tự đổi role
        if (user.Id == adminGuid)
            return ApiResponse<object>.Fail("Không thể thay đổi role của chính mình");

        var validRoles = new[] { "Customer", "Staff" }; // ❌ không cho set Admin
        if (!validRoles.Contains(request.Role))
            return ApiResponse<object>.Fail("Role không hợp lệ. Chỉ chấp nhận: Customer, Staff");

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