using MediatR;
using Qtemplate.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qtemplate.Application.Features.Community.Commands.ToggleLike
{
    public class ToggleLikeCommand : IRequest<ApiResponse<bool>>
    {
        public int PostId { get; set; }
        public Guid UserId { get; set; }
    }
}
