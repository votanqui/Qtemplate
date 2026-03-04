using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.DTOs.User;
using Qtemplate.Application.Services.Interfaces;
using Qtemplate.Domain.Interfaces.Repositories;

namespace Qtemplate.Application.Features.UserManagement.Commands.UpdateProfile;

public class UpdateProfileHandler : IRequestHandler<UpdateProfileCommand, ApiResponse<UserProfileDto>>
{
    private readonly IUserRepository _userRepo;
    private readonly IAuditLogService _auditLogService;

    public UpdateProfileHandler(IUserRepository userRepo, IAuditLogService auditLogService)
    {
        _userRepo = userRepo;
        _auditLogService = auditLogService;
    }

    public async Task<ApiResponse<UserProfileDto>> Handle(UpdateProfileCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepo.GetByIdAsync(request.UserId);
        if (user is null)
            return ApiResponse<UserProfileDto>.Fail("Không tìm thấy người dùng");

        var oldValues = new { user.FullName, user.PhoneNumber };

        user.FullName = request.FullName;
        user.PhoneNumber = request.PhoneNumber;
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepo.UpdateAsync(user);

        await _auditLogService.LogAsync(
            userId: user.Id.ToString(),
            userEmail: user.Email,
            action: "UpdateProfile",
            entityName: "User",
            entityId: user.Id.ToString(),
            oldValues: oldValues,
            newValues: new { user.FullName, user.PhoneNumber },
            ipAddress: request.IpAddress
        );

        return ApiResponse<UserProfileDto>.Ok(new UserProfileDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            AvatarUrl = user.AvatarUrl,
            Role = user.Role,
            IsEmailVerified = user.IsEmailVerified,
            LastLoginAt = user.LastLoginAt,
            CreatedAt = user.CreatedAt
        }, "Cập nhật thông tin thành công");
    }
}
