using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.DTOs.Post;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qtemplate.Application.Features.Posts.Commands.CreatePost
{
    public class CreatePostCommand : IRequest<ApiResponse<AdminPostDto>>
    {
        public UpsertPostDto Data { get; set; } = null!;
        public string AuthorId { get; set; } = string.Empty;
        public string AuthorName { get; set; } = string.Empty;
    }
}
