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

namespace Qtemplate.Application.Features.Affiliates.Queries.GetAdminAffiliates
{
    public class GetAdminAffiliatesHandler
        : IRequestHandler<GetAdminAffiliatesQuery, ApiResponse<PaginatedResult<AffiliateDto>>>
    {
        private readonly IAffiliateRepository _affiliateRepo;

        public GetAdminAffiliatesHandler(IAffiliateRepository affiliateRepo)
            => _affiliateRepo = affiliateRepo;

        public async Task<ApiResponse<PaginatedResult<AffiliateDto>>> Handle(
            GetAdminAffiliatesQuery request, CancellationToken cancellationToken)
        {
            var (items, total) = await _affiliateRepo.GetAdminListAsync(
                request.IsActive, request.Page, request.PageSize);

            return ApiResponse<PaginatedResult<AffiliateDto>>.Ok(new PaginatedResult<AffiliateDto>
            {
                Items = items.Select(a => RegisterAffiliateHandler.ToDto(a)).ToList(),
                TotalCount = total,
                Page = request.Page,
                PageSize = request.PageSize
            });
        }
    }
}
