using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.DTOs.Post;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qtemplate.Application.Features.Posts.Queries.AdminGetPosts
{
    public class AdminGetPostsQuery : IRequest<PaginatedResult<AdminPostDto>>
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public string? Search { get; set; }
        public string? Status { get; set; }  // Draft / Published / Archived / null = all
    }
}
