using MediatR;
using Qtemplate.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qtemplate.Application.Features.Posts.Commands.DeletePost
{
    public class DeletePostCommand : IRequest<ApiResponse<bool>>
    {
        public int Id { get; set; }
    }
}
