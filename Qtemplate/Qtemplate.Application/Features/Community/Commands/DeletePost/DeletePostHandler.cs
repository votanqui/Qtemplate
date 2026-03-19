using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.Services.Interfaces;
using Qtemplate.Domain.Interfaces.Repositories;

namespace Qtemplate.Application.Features.Community.Commands.DeletePost;

public class DeletePostHandler : IRequestHandler<DeletePostCommand, ApiResponse<object>>
{
    private readonly ICommunityRepository _repo;
    private readonly IFileUploadService _fileUploadService;
    private readonly ICommunityRealtimeService _realtime;

    public DeletePostHandler(
        ICommunityRepository repo,
        IFileUploadService fileUploadService,
        ICommunityRealtimeService realtime)
    {
        _repo = repo;
        _fileUploadService = fileUploadService;
        _realtime = realtime;
    }

    public async Task<ApiResponse<object>> Handle(
        DeletePostCommand request, CancellationToken cancellationToken)
    {
        var post = await _repo.GetByIdAsync(request.PostId);
        if (post is null)
            return ApiResponse<object>.Fail("Bài viết không tồn tại");

        if (post.UserId != request.UserId)
            return ApiResponse<object>.Fail("Bạn không có quyền xóa bài viết này");

        if (!string.IsNullOrWhiteSpace(post.ImageUrl))
            _fileUploadService.DeletePostImage(post.ImageUrl);

        await _repo.DeletePostAsync(post);

        // Realtime: xóa bài khỏi feed của mọi người
        await _realtime.BroadcastPostDeletedAsync(request.PostId);

        return ApiResponse<object>.Ok(null!, "Đã xóa bài viết");
    }
}