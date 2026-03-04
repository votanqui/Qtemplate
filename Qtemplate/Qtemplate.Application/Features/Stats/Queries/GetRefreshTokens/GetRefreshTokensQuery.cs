using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.DTOs.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qtemplate.Application.Features.Stats.Queries.GetRefreshTokens
{
    public class GetRefreshTokensQuery : IRequest<ApiResponse<RefreshTokenStatsDto>> { }
}
