// Qtemplate.Application/Features/Community/Commands/HidePost/HidePostHandler.cs

using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.Services.Interfaces;
using Qtemplate.Domain.Interfaces.Repositories;

namespace Qtemplate.Application.Features.Community.Commands.HidePost;

public class HidePostHandler : IRequestHandler<HidePostCommand, ApiResponse<object>>
{
    private readonly ICommunityRepository _repo;
    private readonly INotificationService _notifService;

    public HidePostHandler(ICommunityRepository repo, INotificationService notifService)
    {
        _repo = repo;
        _notifService = notifService;
    }

    public async Task<ApiResponse<object>> Handle(
        HidePostCommand request, CancellationToken cancellationToken)
    {
        var post = await _repo.GetByIdAsync(request.PostId);
        if (post is null)
            return ApiResponse<object>.Fail("Bài viết không tồn tại");

        post.IsHidden = request.IsHidden;
        post.HideReason = request.Reason?.Trim();
        post.UpdatedAt = DateTime.UtcNow;

        await _repo.UpdatePostAsync(post);

        // ── Thông báo đến chủ bài viết ────────────────────────────────────
        var preview = (post.Content?.Length ?? 0) > 50
            ? post.Content![..50] + "…"
            : (post.Content ?? "bài viết của bạn");

        if (request.IsHidden)
        {
            var reason = string.IsNullOrWhiteSpace(request.Reason)
                ? "Vi phạm tiêu chuẩn cộng đồng"
                : request.Reason.Trim();

            await _notifService.SendToUserAsync(
                post.UserId,
                "⚠️ Bài viết của bạn đã bị ẩn",
                $"\"{preview}\" — Lý do: {reason}",
                type: "Warning",
                redirectUrl: "/cong-dong");
        }
        else
        {
            await _notifService.SendToUserAsync(
                post.UserId,
                "✅ Bài viết của bạn đã được hiển thị trở lại",
                $"\"{preview}\"",
                type: "Success",
                redirectUrl: "/cong-dong");
        }

        return ApiResponse<object>.Ok(null!,
            request.IsHidden ? "Đã ẩn bài viết" : "Đã hiện lại bài viết");
    }
}