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

namespace Qtemplate.Application.Features.Community.Queries.AdminGetComments
{
    public class AdminGetCommentsHandler
       : IRequestHandler<AdminGetCommentsQuery, ApiResponse<PaginatedResult<AdminCommunityCommentDto>>>
    {
        private readonly ICommunityRepository _repo;

        public AdminGetCommentsHandler(ICommunityRepository repo) => _repo = repo;

        public async Task<ApiResponse<PaginatedResult<AdminCommunityCommentDto>>> Handle(
            AdminGetCommentsQuery request, CancellationToken cancellationToken)
        {
            var (items, total) = await _repo.GetAdminCommentsAsync(
                request.Page, request.PageSize, request.IsHidden);

            var result = new PaginatedResult<AdminCommunityCommentDto>
            {
                Items = items.Select(CommunityMapper.ToAdminCommentDto).ToList(),
                TotalCount = total,
                Page = request.Page,
                PageSize = request.PageSize,
            };

            return ApiResponse<PaginatedResult<AdminCommunityCommentDto>>.Ok(result);
        }
    }
}
