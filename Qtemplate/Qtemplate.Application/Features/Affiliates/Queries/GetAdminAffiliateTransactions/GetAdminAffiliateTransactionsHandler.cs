using MediatR;
using Qtemplate.Application.DTOs.Affiliate;
using Qtemplate.Application.DTOs;
using Qtemplate.Domain.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qtemplate.Application.Features.Affiliates.Queries.GetAdminAffiliateTransactions
{
    public class GetAdminAffiliateTransactionsHandler
        : IRequestHandler<GetAdminAffiliateTransactionsQuery, ApiResponse<AdminAffiliateTransactionsDto>>
    {
        private readonly IAffiliateRepository _affiliateRepo;
        public GetAdminAffiliateTransactionsHandler(IAffiliateRepository affiliateRepo)
            => _affiliateRepo = affiliateRepo;

        public async Task<ApiResponse<AdminAffiliateTransactionsDto>> Handle(
            GetAdminAffiliateTransactionsQuery request, CancellationToken cancellationToken)
        {
            var (affiliate, txList, total) = await _affiliateRepo.GetAdminTransactionsPagedAsync(
                request.AffiliateId, request.Status, request.Page, request.PageSize);

            if (affiliate is null)
                return ApiResponse<AdminAffiliateTransactionsDto>.Fail("Không tìm thấy affiliate");

            var items = txList.Select(t => new AffiliateTransactionDto
            {
                Id = t.Id,
                OrderId = t.OrderId,
                OrderCode = t.Order?.OrderCode,
                OrderAmount = t.OrderAmount,
                Commission = t.Commission,
                Status = t.Status,
                PaidAt = t.PaidAt,
                CreatedAt = t.CreatedAt,
            }).ToList();

            return ApiResponse<AdminAffiliateTransactionsDto>.Ok(new AdminAffiliateTransactionsDto
            {
                AffiliateId = affiliate.Id,
                AffiliateCode = affiliate.AffiliateCode,
                UserEmail = affiliate.User.Email,
                UserName = affiliate.User.FullName,
                CommissionRate = affiliate.CommissionRate,
                TotalEarned = affiliate.TotalEarned,
                PendingAmount = affiliate.PendingAmount,
                PaidAmount = affiliate.PaidAmount,
                Transactions = new PaginatedResult<AffiliateTransactionDto>
                {
                    Items = items,
                    TotalCount = total,
                    Page = request.Page,
                    PageSize = request.PageSize,
                }
            });
        }
    }
}
