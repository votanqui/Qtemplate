using MediatR;
using Qtemplate.Application.DTOs.User;
using Qtemplate.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qtemplate.Application.Features.UserManagement.Queries.DownloadHistoryQuery
{
    public class GetDownloadHistoryQuery : IRequest<ApiResponse<PagedResultDto<DownloadHistoryItemDto>>>
    {
        public Guid UserId { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
