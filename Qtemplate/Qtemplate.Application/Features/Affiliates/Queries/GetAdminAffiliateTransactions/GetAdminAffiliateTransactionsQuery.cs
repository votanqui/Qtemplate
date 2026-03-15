using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.DTOs.Affiliate;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qtemplate.Application.Features.Affiliates.Queries.GetAdminAffiliateTransactions
{
    public class GetAdminAffiliateTransactionsQuery : IRequest<ApiResponse<AdminAffiliateTransactionsDto>>
    {
        public int AffiliateId { get; set; }
        public string? Status { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }
}
