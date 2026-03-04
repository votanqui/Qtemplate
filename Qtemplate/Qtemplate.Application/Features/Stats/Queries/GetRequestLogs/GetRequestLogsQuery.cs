using MediatR;
using Qtemplate.Application.DTOs.Request;
using Qtemplate.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Qtemplate.Application.DTOs.Admin;

namespace Qtemplate.Application.Features.Stats.Queries.GetRequestLogs
{
    public class GetRequestLogsQuery : IRequest<ApiResponse<RequestLogStatsDto>>
    {
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
    }
}
