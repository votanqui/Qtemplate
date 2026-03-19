using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.DTOs.Community;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qtemplate.Application.Features.Community.Commands.CreatePost
{
    public class CreatePostCommand : IRequest<ApiResponse<CommunityPostDto>>
    {
        public Guid UserId { get; set; }
        public string Content { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
    }
}
