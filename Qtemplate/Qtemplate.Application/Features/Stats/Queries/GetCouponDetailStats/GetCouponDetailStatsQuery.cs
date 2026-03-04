using MediatR;
using Qtemplate.Application.DTOs.Stats;
using Qtemplate.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qtemplate.Application.Features.Stats.Queries.GetCouponDetailStats
{
    public class GetCouponDetailStatsQuery : IRequest<ApiResponse<CouponDetailStatsDto>> { }
}
