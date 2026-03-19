using MediatR;
using Qtemplate.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qtemplate.Application.Features.Community.Commands.HidePost
{
    public class HidePostCommand : IRequest<ApiResponse<object>>
    {
        public int PostId { get; set; }
        public bool IsHidden { get; set; }
        public string? Reason { get; set; }
    }
}
