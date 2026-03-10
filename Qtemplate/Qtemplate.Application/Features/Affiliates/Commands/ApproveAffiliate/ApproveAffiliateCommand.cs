using MediatR;
using Qtemplate.Application.DTOs.Affiliate;
using Qtemplate.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qtemplate.Application.Features.Affiliates.Commands.ApproveAffiliate
{
    public class ApproveAffiliateCommand : IRequest<ApiResponse<AffiliateDto>>
    {
        public int AffiliateId { get; set; }
        public bool IsActive { get; set; }

    }
}
