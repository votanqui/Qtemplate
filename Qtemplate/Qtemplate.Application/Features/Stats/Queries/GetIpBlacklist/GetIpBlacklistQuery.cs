using MediatR;
using Qtemplate.Application.DTOs.IpBlacklist;
using Qtemplate.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Qtemplate.Application.DTOs.Admin;

namespace Qtemplate.Application.Features.Stats.Queries.GetIpBlacklist
{
    public class GetIpBlacklistQuery : IRequest<ApiResponse<IpBlacklistStatsDto>> { }
}
