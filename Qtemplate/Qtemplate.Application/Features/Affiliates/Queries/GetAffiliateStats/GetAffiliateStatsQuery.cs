using MediatR;
using Qtemplate.Application.DTOs.Affiliate;
using Qtemplate.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qtemplate.Application.Features.Affiliates.Queries.GetAffiliateStats
{
    public class GetAffiliateStatsQuery : IRequest<ApiResponse<AffiliateDto>>
    {
        public Guid UserId { get; set; }
    }

}
