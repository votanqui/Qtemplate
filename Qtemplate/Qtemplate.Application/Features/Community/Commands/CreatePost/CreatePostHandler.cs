using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.DTOs.Community;
using Qtemplate.Application.Mappers;
using Qtemplate.Application.Services.Interfaces;
using Qtemplate.Domain.Entities;
using Qtemplate.Domain.Interfaces.Repositories;

namespace Qtemplate.Application.Features.Community.Commands.CreatePost;

public class CreatePostHandler : IRequestHandler<CreatePostCommand, ApiResponse<CommunityPostDto>>
{
    private readonly ICommunityRepository _repo;
    private readonly ICommunityRealtimeService _realtime;

    public CreatePostHandler(ICommunityRepository repo, ICommunityRealtimeService realtime)
    {
        _repo = repo;
        _realtime = realtime;
    }

    public async Task<ApiResponse<CommunityPostDto>> Handle(
        CreatePostCommand request, CancellationToken cancellationToken)
    {
        var post = new CommunityPost
        {
            UserId = request.UserId,
            Content = request.Content.Trim(),
            ImageUrl = request.ImageUrl?.Trim(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        await _repo.AddPostAsync(post);

        // Load lại để có navigation User
        var loaded = await _repo.GetByIdAsync(post.Id);
        var dto = CommunityMapper.ToPostDto(loaded!, request.UserId);

        // Realtime: broadcast bài mới đến tất cả user đang xem feed
        await _realtime.BroadcastNewPostAsync(dto);

        return ApiResponse<CommunityPostDto>.Ok(dto, "Đăng bài thành công");
    }
}