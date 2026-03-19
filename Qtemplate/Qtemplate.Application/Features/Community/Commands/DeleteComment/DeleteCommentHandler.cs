using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.Services.Interfaces;
using Qtemplate.Domain.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qtemplate.Application.Features.Community.Commands.DeleteComment
{
    public class DeleteCommentHandler : IRequestHandler<DeleteCommentCommand, ApiResponse<object>>
    {
        private readonly ICommunityRepository _repo;
        private readonly ICommunityRealtimeService _realtime;

        public DeleteCommentHandler(ICommunityRepository repo, ICommunityRealtimeService realtime)
        {
            _repo = repo;
            _realtime = realtime;
        }

        public async Task<ApiResponse<object>> Handle(
            DeleteCommentCommand request, CancellationToken cancellationToken)
        {
            var comment = await _repo.GetCommentByIdAsync(request.CommentId);
            if (comment is null)
                return ApiResponse<object>.Fail("Bình luận không tồn tại");

            if (comment.UserId != request.UserId)
                return ApiResponse<object>.Fail("Bạn không có quyền xóa bình luận này");

            var postId = comment.PostId;
            var post = await _repo.GetByIdAsync(postId);
            if (post is not null)
            {
                post.CommentCount = Math.Max(0, post.CommentCount - 1);
                await _repo.UpdatePostAsync(post);
            }

            await _repo.DeleteCommentAsync(comment);

            // Realtime: notify user đang xem post này
            await _realtime.BroadcastCommentDeletedAsync(request.CommentId, postId);

            return ApiResponse<object>.Ok(null!, "Đã xóa bình luận");
        }
    }
}
