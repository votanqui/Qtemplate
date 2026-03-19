// Qtemplate.Application/Features/Community/Commands/ToggleLike/ToggleLikeHandler.cs

using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.Services.Interfaces;
using Qtemplate.Domain.Entities;
using Qtemplate.Domain.Interfaces.Repositories;

namespace Qtemplate.Application.Features.Community.Commands.ToggleLike;

public class ToggleLikeHandler : IRequestHandler<ToggleLikeCommand, ApiResponse<bool>>
{
    private readonly ICommunityRepository _repo;
    private readonly ICommunityRealtimeService _realtime;
    private readonly INotificationService _notifService;
    private readonly IUserRepository _userRepo;

    public ToggleLikeHandler(
        ICommunityRepository repo,
        ICommunityRealtimeService realtime,
        INotificationService notifService,
        IUserRepository userRepo)
    {
        _repo = repo;
        _realtime = realtime;
        _notifService = notifService;
        _userRepo = userRepo;
    }

    public async Task<ApiResponse<bool>> Handle(
        ToggleLikeCommand request, CancellationToken cancellationToken)
    {
        var post = await _repo.GetByIdAsync(request.PostId);
        if (post is null) return ApiResponse<bool>.Fail("Bài viết không tồn tại");
        if (post.IsHidden) return ApiResponse<bool>.Fail("Bài viết không khả dụng");

        var existing = await _repo.GetLikeAsync(request.PostId, request.UserId);
        bool isNowLiked;

        if (existing is not null)
        {
            await _repo.RemoveLikeAsync(existing);
            post.LikeCount = Math.Max(0, post.LikeCount - 1);
            isNowLiked = false;
        }
        else
        {
            await _repo.AddLikeAsync(new CommunityLike
            {
                PostId = request.PostId,
                UserId = request.UserId,
                CreatedAt = DateTime.UtcNow,
            });
            post.LikeCount++;
            isNowLiked = true;
        }

        await _repo.UpdatePostAsync(post);

        // Realtime broadcast
        await _realtime.BroadcastLikeUpdatedAsync(post.Id, post.LikeCount, isNowLiked);

        // ── Notification: chỉ gửi khi LIKE (không gửi khi unlike), không tự like bài mình ──
        if (isNowLiked && post.UserId != request.UserId)
        {
            var liker = await _userRepo.GetByIdAsync(request.UserId);
            var likerName = liker?.FullName ?? "Ai đó";

            // Giới hạn nội dung bài viết preview
            var postPreview = (post.Content?.Length ?? 0) > 40
                ? post.Content![..40] + "…"
                : (post.Content ?? "bài viết của bạn");

            await _notifService.SendToUserAsync(
                post.UserId,
                $"{likerName} đã thích bài viết của bạn",
                $"\"{postPreview}\"",
                type: "Info",
                redirectUrl: "/cong-dong");
        }

        return ApiResponse<bool>.Ok(isNowLiked,
            isNowLiked ? "Đã thích bài viết" : "Đã bỏ thích");
    }
}