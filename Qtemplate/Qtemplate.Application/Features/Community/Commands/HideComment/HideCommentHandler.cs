// Qtemplate.Application/Features/Community/Commands/HideComment/HideCommentHandler.cs

using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.Services.Interfaces;
using Qtemplate.Domain.Interfaces.Repositories;

namespace Qtemplate.Application.Features.Community.Commands.HideComment;

public class HideCommentHandler : IRequestHandler<HideCommentCommand, ApiResponse<object>>
{
    private readonly ICommunityRepository _repo;
    private readonly INotificationService _notifService;

    public HideCommentHandler(ICommunityRepository repo, INotificationService notifService)
    {
        _repo = repo;
        _notifService = notifService;
    }

    public async Task<ApiResponse<object>> Handle(
        HideCommentCommand request, CancellationToken cancellationToken)
    {
        var comment = await _repo.GetCommentByIdAsync(request.CommentId);
        if (comment is null)
            return ApiResponse<object>.Fail("Bình luận không tồn tại");

        comment.IsHidden = request.IsHidden;
        comment.UpdatedAt = DateTime.UtcNow;

        await _repo.UpdateCommentAsync(comment);

        // ── Thông báo đến chủ bình luận ───────────────────────────────────
        var preview = comment.Content.Length > 60
            ? comment.Content[..60] + "…"
            : comment.Content;

        if (request.IsHidden)
        {
            await _notifService.SendToUserAsync(
                comment.UserId,
                "⚠️ Bình luận của bạn đã bị ẩn",
                $"\"{preview}\" — Vi phạm tiêu chuẩn cộng đồng",
                type: "Warning",
                redirectUrl: "/cong-dong");
        }
        else
        {
            await _notifService.SendToUserAsync(
                comment.UserId,
                "✅ Bình luận của bạn đã được hiển thị trở lại",
                $"\"{preview}\"",
                type: "Success",
                redirectUrl: "/cong-dong");
        }

        return ApiResponse<object>.Ok(null!,
            request.IsHidden ? "Đã ẩn bình luận" : "Đã hiện lại bình luận");
    }
}