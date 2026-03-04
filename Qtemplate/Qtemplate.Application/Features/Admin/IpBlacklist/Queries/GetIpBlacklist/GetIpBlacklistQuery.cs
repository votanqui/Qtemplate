using MediatR;
using Qtemplate.Application.DTOs.IpBlacklist;
using Qtemplate.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qtemplate.Application.Features.Admin.IpBlacklist.Queries.GetIpBlacklist
{
    public class GetIpBlacklistQuery : IRequest<ApiResponse<PaginatedResult<IpBlacklistDto>>>
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
