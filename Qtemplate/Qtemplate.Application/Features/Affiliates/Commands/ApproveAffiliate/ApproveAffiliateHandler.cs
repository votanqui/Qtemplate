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

namespace Qtemplate.Application.Features.Affiliates.Commands.ApproveAffiliate
{
    public class ApproveAffiliateHandler
        : IRequestHandler<ApproveAffiliateCommand, ApiResponse<AffiliateDto>>
    {
        private readonly IAffiliateRepository _affiliateRepo;

        public ApproveAffiliateHandler(IAffiliateRepository affiliateRepo)
            => _affiliateRepo = affiliateRepo;

        public async Task<ApiResponse<AffiliateDto>> Handle(
            ApproveAffiliateCommand request, CancellationToken cancellationToken)
        {
            var affiliate = await _affiliateRepo.GetByIdAsync(request.AffiliateId);
            if (affiliate is null)
                return ApiResponse<AffiliateDto>.Fail("Không tìm thấy affiliate");

            if (request.CommissionRate is < 1 or > 50)
                return ApiResponse<AffiliateDto>.Fail("CommissionRate phải từ 1% đến 50%");

            affiliate.IsActive = request.IsActive;
            affiliate.CommissionRate = request.CommissionRate;

            await _affiliateRepo.UpdateAsync(affiliate);

            return ApiResponse<AffiliateDto>.Ok(
                RegisterAffiliateHandler.ToDto(affiliate),
                request.IsActive ? "Đã duyệt affiliate" : "Đã vô hiệu hoá affiliate");
        }
    }
}
