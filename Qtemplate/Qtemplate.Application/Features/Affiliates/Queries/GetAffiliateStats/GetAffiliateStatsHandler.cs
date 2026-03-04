using MediatR;
using Qtemplate.Application.DTOs.Affiliate;
using Qtemplate.Application.DTOs;
using Qtemplate.Application.Features.Affiliates.Commands.RegisterAffiliate;
using Qtemplate.Domain.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qtemplate.Application.Features.Affiliates.Queries.GetAffiliateStats
{
    public class GetAffiliateStatsHandler
     : IRequestHandler<GetAffiliateStatsQuery, ApiResponse<AffiliateDto>>
    {
        private readonly IAffiliateRepository _affiliateRepo;

        public GetAffiliateStatsHandler(IAffiliateRepository affiliateRepo)
            => _affiliateRepo = affiliateRepo;

        public async Task<ApiResponse<AffiliateDto>> Handle(
            GetAffiliateStatsQuery request, CancellationToken cancellationToken)
        {
            var affiliate = await _affiliateRepo.GetByUserIdAsync(request.UserId);
            if (affiliate is null)
                return ApiResponse<AffiliateDto>.Fail("Bạn chưa đăng ký affiliate");

            var transactions = await _affiliateRepo
                .GetTransactionsByAffiliateIdAsync(affiliate.Id);

            affiliate.Transactions = transactions;

            return ApiResponse<AffiliateDto>.Ok(RegisterAffiliateHandler.ToDto(affiliate));
        }
    }

}
