using MediatR;
using Qtemplate.Application.DTOs.IpBlacklist;
using Qtemplate.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qtemplate.Application.Features.Admin.IpBlacklist.Commands.AddIpBlacklist
{
    public class AddIpBlacklistCommand : IRequest<ApiResponse<IpBlacklistDto>>
    {
        public string IpAddress { get; set; } = string.Empty;
        public string? Reason { get; set; }
        public DateTime? ExpiredAt { get; set; }
        public string? AdminEmail { get; set; }
    }
}
