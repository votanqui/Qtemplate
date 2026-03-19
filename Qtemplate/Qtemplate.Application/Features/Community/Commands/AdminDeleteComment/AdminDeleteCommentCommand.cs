using MediatR;
using Qtemplate.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qtemplate.Application.Features.Community.Commands.AdminDeleteComment
{
    public class AdminDeleteCommentCommand : IRequest<ApiResponse<object>>
    {
        public int CommentId { get; set; }
    }
}
