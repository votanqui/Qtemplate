// Qtemplate.Application/Features/Community/Commands/AdminDeletePost/AdminDeletePostHandler.cs

using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.Services.Interfaces;
using Qtemplate.Domain.Interfaces.Repositories;

namespace Qtemplate.Application.Features.Community.Commands.AdminDeletePost;

public class AdminDeletePostHandler : IRequestHandler<AdminDeletePostCommand, ApiResponse<object>>
{
    private readonly ICommunityRepository _repo;
    private readonly IFileUploadService _fileUploadService;
    private readonly INotificationService _notifService;

    public AdminDeletePostHandler(
        ICommunityRepository repo,
        IFileUploadService fileUploadService,
        INotificationService notifService)
    {
        _repo = repo;
        _fileUploadService = fileUploadService;
        _notifService = notifService;
    }

    public async Task<ApiResponse<object>> Handle(
        AdminDeletePostCommand request, CancellationToken cancellationToken)
    {
        var post = await _repo.GetByIdAsync(request.PostId);
        if (post is null)
            return ApiResponse<object>.Fail("Bài viết không tồn tại");

        var ownerId = post.UserId;
        var preview = (post.Content?.Length ?? 0) > 50
            ? post.Content![..50] + "…"
            : (post.Content ?? "bài viết của bạn");

        // Xóa ảnh đính kèm khỏi disk
        if (!string.IsNullOrWhiteSpace(post.ImageUrl))
            _fileUploadService.DeletePostImage(post.ImageUrl);

        await _repo.DeletePostAsync(post);

        // ── Thông báo đến chủ bài viết ────────────────────────────────────
        await _notifService.SendToUserAsync(
            ownerId,
            "🗑️ Bài viết của bạn đã bị xóa bởi quản trị viên",
            $"\"{preview}\" — Vi phạm tiêu chuẩn cộng đồng",
            type: "Warning",
            redirectUrl: "/cong-dong");

        return ApiResponse<object>.Ok(null!, "Đã xóa bài viết");
    }
}