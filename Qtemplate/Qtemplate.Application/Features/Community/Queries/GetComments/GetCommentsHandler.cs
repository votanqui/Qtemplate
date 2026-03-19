using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.DTOs.Community;
using Qtemplate.Application.Mappers;
using Qtemplate.Domain.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qtemplate.Application.Features.Community.Queries.GetComments
{
    public class GetCommentsHandler : IRequestHandler<GetCommentsQuery, ApiResponse<PaginatedResult<CommunityCommentDto>>>
    {
        private readonly ICommunityRepository _repo;

        public GetCommentsHandler(ICommunityRepository repo) => _repo = repo;

        public async Task<ApiResponse<PaginatedResult<CommunityCommentDto>>> Handle(
            GetCommentsQuery request, CancellationToken cancellationToken)
        {
            var post = await _repo.GetByIdAsync(request.PostId);
            if (post is null || post.IsHidden)
                return ApiResponse<PaginatedResult<CommunityCommentDto>>.Fail("Bài viết không tồn tại");

            var (items, total) = await _repo.GetCommentsAsync(
                request.PostId, request.Page, request.PageSize);

            var result = new PaginatedResult<CommunityCommentDto>
            {
                Items = items.Select(c => CommunityMapper.ToCommentDto(c, request.CurrentUserId)).ToList(),
                TotalCount = total,
                Page = request.Page,
                PageSize = request.PageSize,
            };

            return ApiResponse<PaginatedResult<CommunityCommentDto>>.Ok(result);
        }
    }
}
