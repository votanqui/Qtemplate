using MediatR;
using Qtemplate.Application.DTOs.Email;
using Qtemplate.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qtemplate.Application.Features.Admin.Logs.Queries.GetEmailLogs
{
    public class GetEmailLogsQuery : IRequest<ApiResponse<PaginatedResult<EmailLogDto>>>
    {
        public string? Status { get; set; }
        public string? Template { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
