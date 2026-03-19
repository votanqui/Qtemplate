using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.DTOs.Community;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qtemplate.Application.Features.Community.Queries.GetFeed
{
    public class GetFeedQuery : IRequest<ApiResponse<PaginatedResult<CommunityPostDto>>>
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public Guid? CurrentUserId { get; set; }
    }
}
