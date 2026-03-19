using MediatR;
using Qtemplate.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qtemplate.Application.Features.Community.Commands.HideComment
{
    public class HideCommentCommand : IRequest<ApiResponse<object>>
    {
        public int CommentId { get; set; }
        public bool IsHidden { get; set; }
    }
}
