using MediatR;
using Qtemplate.Application.DTOs.Email;
using Qtemplate.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Qtemplate.Application.DTOs.Admin;

namespace Qtemplate.Application.Features.Stats.Queries.GetEmailLogs
{
    public class GetEmailLogsQuery : IRequest<ApiResponse<EmailLogStatsDto>>
    {
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
    }
}
