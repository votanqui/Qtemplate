using MediatR;
using Qtemplate.Application.DTOs.RefreshToken;
using Qtemplate.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qtemplate.Application.Features.Admin.Logs.Queries.GetRefreshTokens
{
    public class GetRefreshTokensQuery : IRequest<ApiResponse<PaginatedResult<RefreshTokenDto>>>
    {
        public Guid? UserId { get; set; }
        public bool? IsActive { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
