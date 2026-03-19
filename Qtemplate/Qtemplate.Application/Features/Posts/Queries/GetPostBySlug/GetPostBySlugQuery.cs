using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.DTOs.Post;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qtemplate.Application.Features.Posts.Queries.GetPostBySlug
{
    public class GetPostBySlugQuery : IRequest<ApiResponse<PostDetailDto>>
    {
        public string Slug { get; set; } = string.Empty;
    }
}
