// Qtemplate.Application/Features/Community/Commands/AdminDeleteComment/AdminDeleteCommentHandler.cs

using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.Services.Interfaces;
using Qtemplate.Domain.Interfaces.Repositories;

namespace Qtemplate.Application.Features.Community.Commands.AdminDeleteComment;

public class AdminDeleteCommentHandler : IRequestHandler<AdminDeleteCommentCommand, ApiResponse<object>>
{
    private readonly ICommunityRepository _repo;
    private readonly INotificationService _notifService;

    public AdminDeleteCommentHandler(
        ICommunityRepository repo,
        INotificationService notifService)
    {
        _repo = repo;
        _notifService = notifService;
    }

    public async Task<ApiResponse<object>> Handle(
        AdminDeleteCommentCommand request, CancellationToken cancellationToken)
    {
        var comment = await _repo.GetCommentByIdAsync(request.CommentId);
        if (comment is null)
            return ApiResponse<object>.Fail("Bình luận không tồn tại");

        var ownerId = comment.UserId;
        var preview = comment.Content.Length > 60
            ? comment.Content[..60] + "…"
            : comment.Content;

        // Giảm CommentCount trên post
        var post = await _repo.GetByIdAsync(comment.PostId);
        if (post is not null)
        {
            post.CommentCount = Math.Max(0, post.CommentCount - 1);
            await _repo.UpdatePostAsync(post);
        }

        await _repo.DeleteCommentAsync(comment);

        // ── Thông báo đến chủ bình luận ───────────────────────────────────
        await _notifService.SendToUserAsync(
            ownerId,
            "🗑️ Bình luận của bạn đã bị xóa bởi quản trị viên",
            $"\"{preview}\" — Vi phạm tiêu chuẩn cộng đồng",
            type: "Warning",
            redirectUrl: "/cong-dong");

        return ApiResponse<object>.Ok(null!, "Đã xóa bình luận");
    }
}