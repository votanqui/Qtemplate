using MediatR;
using Qtemplate.Application.DTOs.Stats;
using Qtemplate.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qtemplate.Application.Features.Stats.Queries.GetDailyStats
{
    public class GetDailyStatsQuery : IRequest<ApiResponse<DailyStatsResultDto>>
    {
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        /// <summary>daily | weekly | monthly — mặc định daily</summary>
        public string Period { get; set; } = "daily";
    }

}
