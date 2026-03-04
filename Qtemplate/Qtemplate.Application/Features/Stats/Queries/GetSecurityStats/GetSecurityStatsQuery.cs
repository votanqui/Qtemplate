using MediatR;
using Qtemplate.Application.DTOs.Admin;
using Qtemplate.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qtemplate.Application.Features.Stats.Queries.GetSecurityStats
{
    public class GetSecurityStatsQuery : IRequest<ApiResponse<SecurityStatsDto>>
    {
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
    }
}
