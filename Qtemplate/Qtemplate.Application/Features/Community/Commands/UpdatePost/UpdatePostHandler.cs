using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.DTOs.Community;
using Qtemplate.Application.Mappers;
using Qtemplate.Application.Services.Interfaces;
using Qtemplate.Domain.Interfaces.Repositories;

namespace Qtemplate.Application.Features.Community.Commands.UpdatePost;

public class UpdatePostHandler : IRequestHandler<UpdatePostCommand, ApiResponse<CommunityPostDto>>
{
    private readonly ICommunityRepository _repo;
    private readonly IFileUploadService _fileUploadService;
    private readonly ICommunityRealtimeService _realtime;

    public UpdatePostHandler(
        ICommunityRepository repo,
        IFileUploadService fileUploadService,
        ICommunityRealtimeService realtime)
    {
        _repo = repo;
        _fileUploadService = fileUploadService;
        _realtime = realtime;
    }

    public async Task<ApiResponse<CommunityPostDto>> Handle(
        UpdatePostCommand request, CancellationToken cancellationToken)
    {
        var post = await _repo.GetByIdAsync(request.PostId);
        if (post is null)
            return ApiResponse<CommunityPostDto>.Fail("Bài viết không tồn tại");

        if (post.UserId != request.UserId)
            return ApiResponse<CommunityPostDto>.Fail("Bạn không có quyền sửa bài viết này");

        var oldImageUrl = post.ImageUrl;
        var imageChanged = oldImageUrl != request.ImageUrl;
        if (imageChanged && !string.IsNullOrWhiteSpace(oldImageUrl))
            _fileUploadService.DeletePostImage(oldImageUrl);

        post.Content = request.Content.Trim();
        post.ImageUrl = request.ImageUrl?.Trim();
        post.UpdatedAt = DateTime.UtcNow;

        await _repo.UpdatePostAsync(post);

        var dto = CommunityMapper.ToPostDto(post, request.UserId);

        // Realtime: notify tất cả user đang xem feed
        await _realtime.BroadcastPostUpdatedAsync(dto);

        return ApiResponse<CommunityPostDto>.Ok(dto, "Cập nhật bài viết thành công");
    }
}