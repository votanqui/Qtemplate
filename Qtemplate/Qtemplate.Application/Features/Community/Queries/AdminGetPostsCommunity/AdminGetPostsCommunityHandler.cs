using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.DTOs.Community;
using Qtemplate.Application.Mappers;
using Qtemplate.Domain.Interfaces.Repositories;

namespace Qtemplate.Application.Features.Community.Queries.AdminGetPostsCommunity
{
    public class AdminGetPostsCommunityHandler
        : IRequestHandler<AdminGetPostsCommunityQuery, ApiResponse<PaginatedResult<AdminCommunityPostDto>>>
    {
        private readonly ICommunityRepository _repo;

        public AdminGetPostsCommunityHandler(ICommunityRepository repo)
        {
            _repo = repo;
        }

        public async Task<ApiResponse<PaginatedResult<AdminCommunityPostDto>>> Handle(
            AdminGetPostsCommunityQuery request,
            CancellationToken cancellationToken)
        {
            var (items, total) = await _repo.GetAdminPostsAsync(
                request.Page,
                request.PageSize,
                request.Search,
                request.IsHidden
            );

            var result = new PaginatedResult<AdminCommunityPostDto>
            {
                Items = items.Select(CommunityMapper.ToAdminPostDto).ToList(),
                TotalCount = total,
                Page = request.Page,
                PageSize = request.PageSize,
            };

            return ApiResponse<PaginatedResult<AdminCommunityPostDto>>.Ok(result);
        }
    }
}