using MediatR;
using Qtemplate.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qtemplate.Application.Features.Community.Commands.DeletePost
{
    public class DeletePostCommand : IRequest<ApiResponse<object>>
    {
        public int PostId { get; set; }
        public Guid UserId { get; set; }
    }
}
