using MediatR;
using Qtemplate.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qtemplate.Application.Features.Admin.IpBlacklist.Commands.ToggleIpBlacklist
{
    public class ToggleIpBlacklistCommand : IRequest<ApiResponse<object>>
    {
        public int Id { get; set; }
    }
}
