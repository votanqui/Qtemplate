// Qtemplate.Application/Features/Community/Commands/CreateComment/CreateCommentHandler.cs

using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.DTOs.Community;
using Qtemplate.Application.Mappers;
using Qtemplate.Application.Services.Interfaces;
using Qtemplate.Domain.Entities;
using Qtemplate.Domain.Interfaces.Repositories;

namespace Qtemplate.Application.Features.Community.Commands.CreateComment;

public class CreateCommentHandler : IRequestHandler<CreateCommentCommand, ApiResponse<CommunityCommentDto>>
{
    private readonly ICommunityRepository _repo;
    private readonly ICommunityRealtimeService _realtime;
    private readonly INotificationService _notifService;

    public CreateCommentHandler(
        ICommunityRepository repo,
        ICommunityRealtimeService realtime,
        INotificationService notifService)
    {
        _repo = repo;
        _realtime = realtime;
        _notifService = notifService;
    }

    public async Task<ApiResponse<CommunityCommentDto>> Handle(
        CreateCommentCommand request, CancellationToken cancellationToken)
    {
        var post = await _repo.GetByIdAsync(request.PostId);
        if (post is null || post.IsHidden)
            return ApiResponse<CommunityCommentDto>.Fail("Bài viết không tồn tại hoặc đã bị ẩn");

        CommunityComment? parentComment = null;
        if (request.ParentId.HasValue)
        {
            parentComment = await _repo.GetCommentByIdAsync(request.ParentId.Value);
            if (parentComment is null || parentComment.PostId != request.PostId)
                return ApiResponse<CommunityCommentDto>.Fail("Comment gốc không tồn tại");
            if (parentComment.ParentId.HasValue)
                return ApiResponse<CommunityCommentDto>.Fail("Chỉ hỗ trợ trả lời 1 cấp");
        }

        var comment = new CommunityComment
        {
            PostId = request.PostId,
            UserId = request.UserId,
            ParentId = request.ParentId,
            Content = request.Content.Trim(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        await _repo.AddCommentAsync(comment);

        post.CommentCount++;
        await _repo.UpdatePostAsync(post);

        var loaded = await _repo.GetCommentByIdAsync(comment.Id);
        var dto = CommunityMapper.ToCommentDto(loaded!, request.UserId);

        // Realtime: broadcast comment mới
        await _realtime.BroadcastNewCommentAsync(dto);

        // ── Notification ──────────────────────────────────────────────────────
        var commenterName = loaded?.User?.FullName ?? "Ai đó";
        var preview = comment.Content.Length > 60
            ? comment.Content[..60] + "…"
            : comment.Content;

        if (request.ParentId.HasValue && parentComment is not null)
        {
            // Reply vào comment → notify chủ comment gốc (nếu không phải chính mình)
            if (parentComment.UserId != request.UserId)
            {
                await _notifService.SendToUserAsync(
                    parentComment.UserId,
                    $"{commenterName} đã trả lời bình luận của bạn",
                    $"\"{preview}\"",
                    type: "Info",
                    redirectUrl: "/cong-dong");
            }
        }
        else
        {
            // Comment mới vào bài → notify chủ bài (nếu không phải chính mình)
            if (post.UserId != request.UserId)
            {
                await _notifService.SendToUserAsync(
                    post.UserId,
                    $"{commenterName} đã bình luận vào bài viết của bạn",
                    $"\"{preview}\"",
                    type: "Info",
                    redirectUrl: "/cong-dong");
            }
        }

        return ApiResponse<CommunityCommentDto>.Ok(dto, "Bình luận thành công");
    }
}