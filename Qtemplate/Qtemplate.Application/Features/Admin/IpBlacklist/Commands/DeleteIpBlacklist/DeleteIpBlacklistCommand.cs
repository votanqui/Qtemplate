using MediatR;
using Qtemplate.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qtemplate.Application.Features.Admin.IpBlacklist.Commands.DeleteIpBlacklist
{
    public class DeleteIpBlacklistCommand : IRequest<ApiResponse<object>>
    {
        public int Id { get; set; }
    }
}
