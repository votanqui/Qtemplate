using MediatR;
using Qtemplate.Application.DTOs.Affiliate;
using Qtemplate.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Qtemplate.Domain.Interfaces.Repositories;

namespace Qtemplate.Application.Features.Affiliates.Queries.GetMyTransactions
{
    public class GetMyAffiliateTransactionsHandler
     : IRequestHandler<GetMyAffiliateTransactionsQuery, ApiResponse<PaginatedResult<AffiliateTransactionDto>>>
    {
        private readonly IAffiliateRepository _affiliateRepo;
        public GetMyAffiliateTransactionsHandler(IAffiliateRepository affiliateRepo)
            => _affiliateRepo = affiliateRepo;

        public async Task<ApiResponse<PaginatedResult<AffiliateTransactionDto>>> Handle(
            GetMyAffiliateTransactionsQuery request, CancellationToken cancellationToken)
        {
            var (txList, total) = await _affiliateRepo.GetMyTransactionsPagedAsync(
                request.UserId, request.Status, request.Page, request.PageSize);

            if (total == 0 && txList.Count == 0)
                return ApiResponse<PaginatedResult<AffiliateTransactionDto>>
                    .Fail("Bạn chưa đăng ký affiliate hoặc chưa có giao dịch nào");

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

            return ApiResponse<PaginatedResult<AffiliateTransactionDto>>.Ok(
                new PaginatedResult<AffiliateTransactionDto>
                {
                    Items = items,
                    TotalCount = total,
                    Page = request.Page,
                    PageSize = request.PageSize,
                });
        }
    }
}
