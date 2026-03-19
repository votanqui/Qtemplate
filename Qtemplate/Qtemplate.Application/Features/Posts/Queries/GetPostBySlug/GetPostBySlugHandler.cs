using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.DTOs.Post;
using Qtemplate.Application.Features.Posts.Queries.GetPublishedPosts;
using Qtemplate.Domain.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qtemplate.Application.Features.Posts.Queries.GetPostBySlug
{
    public class GetPostBySlugHandler : IRequestHandler<GetPostBySlugQuery, ApiResponse<PostDetailDto>>
    {
        private readonly IPostRepository _repo;
        public GetPostBySlugHandler(IPostRepository repo) => _repo = repo;

        public async Task<ApiResponse<PostDetailDto>> Handle(
            GetPostBySlugQuery request, CancellationToken cancellationToken)
        {
            var post = await _repo.GetBySlugAsync(request.Slug);
            if (post == null || post.Status != "Published")
                return ApiResponse<PostDetailDto>.Fail("Bài viết không tồn tại");

            // Tăng view count bất đồng bộ (fire & forget)
            _ = _repo.IncrementViewCountAsync(post.Id);

            return ApiResponse<PostDetailDto>.Ok(
                GetPublishedPostsHandler.ToDetailDto(post));
        }
    }
}
