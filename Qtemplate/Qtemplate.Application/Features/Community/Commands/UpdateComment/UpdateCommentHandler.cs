using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.DTOs.Community;
using Qtemplate.Application.Mappers;
using Qtemplate.Application.Services.Interfaces;
using Qtemplate.Domain.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qtemplate.Application.Features.Community.Commands.UpdateComment
{
    public class UpdateCommentHandler : IRequestHandler<UpdateCommentCommand, ApiResponse<CommunityCommentDto>>
    {
        private readonly ICommunityRepository _repo;
        private readonly ICommunityRealtimeService _realtime;

        public UpdateCommentHandler(ICommunityRepository repo, ICommunityRealtimeService realtime)
        {
            _repo = repo;
            _realtime = realtime;
        }

        public async Task<ApiResponse<CommunityCommentDto>> Handle(
            UpdateCommentCommand request, CancellationToken cancellationToken)
        {
            var comment = await _repo.GetCommentByIdAsync(request.CommentId);
            if (comment is null)
                return ApiResponse<CommunityCommentDto>.Fail("Bình luận không tồn tại");

            if (comment.UserId != request.UserId)
                return ApiResponse<CommunityCommentDto>.Fail("Bạn không có quyền sửa bình luận này");

            comment.Content = request.Content.Trim();
            comment.UpdatedAt = DateTime.UtcNow;
            await _repo.UpdateCommentAsync(comment);

            var dto = CommunityMapper.ToCommentDto(comment, request.UserId);

            // Realtime: notify user đang xem post này
            await _realtime.BroadcastCommentUpdatedAsync(dto);

            return ApiResponse<CommunityCommentDto>.Ok(dto, "Cập nhật bình luận thành công");
        }
    }

}
