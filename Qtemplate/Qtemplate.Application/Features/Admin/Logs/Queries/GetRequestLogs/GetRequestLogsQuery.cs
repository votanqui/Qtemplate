using MediatR;
using Qtemplate.Application.DTOs.Request;
using Qtemplate.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qtemplate.Application.Features.Admin.Logs.Queries.GetRequestLogs
{
    public class GetRequestLogsQuery : IRequest<ApiResponse<PaginatedResult<RequestLogDto>>>
    {
        public string? Ip { get; set; }
        public string? UserId { get; set; }
        public string? Endpoint { get; set; }
        public int? StatusCode { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 50;
    }
}
