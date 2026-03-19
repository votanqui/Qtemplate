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

namespace Qtemplate.Application.Features.Community.Queries.AdminGetPosts
{
    public class AdminGetPostsHandler
        : IRequestHandler<AdminGetPostsQuery, ApiResponse<PaginatedResult<AdminCommunityPostDto>>>
    {
        private readonly ICommunityRepository _repo;

        public AdminGetPostsHandler(ICommunityRepository repo) => _repo = repo;

        public async Task<ApiResponse<PaginatedResult<AdminCommunityPostDto>>> Handle(
            AdminGetPostsQuery request, CancellationToken cancellationToken)
        {
            var (items, total) = await _repo.GetAdminPostsAsync(
                request.Page, request.PageSize, request.Search, request.IsHidden);

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
