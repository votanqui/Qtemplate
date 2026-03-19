using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Domain.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qtemplate.Application.Features.Posts.Commands.DeletePost
{
    public class DeletePostHandler : IRequestHandler<DeletePostCommand, ApiResponse<bool>>
    {
        private readonly IPostRepository _repo;
        public DeletePostHandler(IPostRepository repo) => _repo = repo;

        public async Task<ApiResponse<bool>> Handle(
            DeletePostCommand request, CancellationToken cancellationToken)
        {
            var post = await _repo.GetByIdAsync(request.Id);
            if (post == null)
                return ApiResponse<bool>.Fail("Bài viết không tồn tại");

            await _repo.DeleteAsync(post);
            return ApiResponse<bool>.Ok(true, "Xóa bài viết thành công");
        }
    }
}
