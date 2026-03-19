using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.DTOs.Community;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qtemplate.Application.Features.Community.Commands.CreateComment
{
    public class CreateCommentCommand : IRequest<ApiResponse<CommunityCommentDto>>
    {
        public int PostId { get; set; }
        public Guid UserId { get; set; }
        public string Content { get; set; } = string.Empty;
        public int? ParentId { get; set; }
    }
}
