// Qtemplate.Application/Features/UserManagement/Commands/UpdateAvatar/UpdateAvatarHandler.cs
using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.Services.Interfaces;
using Qtemplate.Domain.Interfaces.Repositories;

namespace Qtemplate.Application.Features.UserManagement.Commands.UpdateAvatar;

public class UpdateAvatarHandler : IRequestHandler<UpdateAvatarCommand, ApiResponse<string>>
{
    private readonly IUserRepository _userRepo;
    private readonly IFileUploadService _fileUploadService;
    private readonly IAuditLogService _auditLogService;

    public UpdateAvatarHandler(
        IUserRepository userRepo,
        IFileUploadService fileUploadService,
        IAuditLogService auditLogService)
    {
        _userRepo = userRepo;
        _fileUploadService = fileUploadService;
        _auditLogService = auditLogService;
    }

    public async Task<ApiResponse<string>> Handle(UpdateAvatarCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepo.GetByIdAsync(request.UserId);
        if (user is null)
            return ApiResponse<string>.Fail("Không tìm thấy người dùng");

        string newAvatarUrl;
        try
        {
            await using var stream = request.File.OpenReadStream();
            newAvatarUrl = await _fileUploadService.SaveAvatarAsync(
                stream,
                request.File.FileName,
                request.File.Length
            );
        }
        catch (InvalidOperationException ex)
        {
            return ApiResponse<string>.Fail(ex.Message);
        }

        // Xóa avatar cũ sau khi lưu thành công
        var oldAvatarUrl = user.AvatarUrl;
        _fileUploadService.DeleteAvatar(oldAvatarUrl);

        user.AvatarUrl = newAvatarUrl;
        user.UpdatedAt = DateTime.UtcNow;
        await _userRepo.UpdateAsync(user);

        await _auditLogService.LogAsync(
            userId: user.Id.ToString(),
            userEmail: user.Email,
            action: "UpdateAvatar",
            entityName: "User",
            entityId: user.Id.ToString(),
            oldValues: new { AvatarUrl = oldAvatarUrl },
            newValues: new { AvatarUrl = newAvatarUrl },
            ipAddress: request.IpAddress
        );

        return ApiResponse<string>.Ok(newAvatarUrl, "Cập nhật avatar thành công");
    }
}