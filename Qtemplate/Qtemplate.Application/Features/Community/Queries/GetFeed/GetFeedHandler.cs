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

namespace Qtemplate.Application.Features.Community.Queries.GetFeed
{
    public class GetFeedHandler : IRequestHandler<GetFeedQuery, ApiResponse<PaginatedResult<CommunityPostDto>>>
    {
        private readonly ICommunityRepository _repo;

        public GetFeedHandler(ICommunityRepository repo) => _repo = repo;

        public async Task<ApiResponse<PaginatedResult<CommunityPostDto>>> Handle(
            GetFeedQuery request, CancellationToken cancellationToken)
        {
            var (items, total) = await _repo.GetFeedAsync(request.Page, request.PageSize);

            // Batch load liked post IDs — tránh N+1 query
            var likedIds = new List<int>();
            if (request.CurrentUserId.HasValue && items.Count > 0)
                likedIds = await _repo.GetLikedPostIdsAsync(
                    items.Select(p => p.Id),
                    request.CurrentUserId.Value);

            var result = new PaginatedResult<CommunityPostDto>
            {
                Items = items.Select(p => CommunityMapper.ToPostDto(p, request.CurrentUserId, likedIds)).ToList(),
                TotalCount = total,
                Page = request.Page,
                PageSize = request.PageSize,
            };

            return ApiResponse<PaginatedResult<CommunityPostDto>>.Ok(result);
        }
    }
}
