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

namespace Qtemplate.Application.Features.Posts.Queries.AdminGetPosts
{
    public class AdminGetPostsHandler : IRequestHandler<AdminGetPostsQuery, PaginatedResult<AdminPostDto>>
    {
        private readonly IPostRepository _repo;
        public AdminGetPostsHandler(IPostRepository repo) => _repo = repo;

        public async Task<PaginatedResult<AdminPostDto>> Handle(
            AdminGetPostsQuery request, CancellationToken cancellationToken)
        {
            var (items, total) = await _repo.GetAdminListAsync(
                request.Page, request.PageSize, request.Search, request.Status);

            return new PaginatedResult<AdminPostDto>
            {
                Items = items.Select(GetPublishedPostsHandler.ToAdminDto).ToList(),
                TotalCount = total,
                Page = request.Page,
                PageSize = request.PageSize
            };
        }
    }
}
