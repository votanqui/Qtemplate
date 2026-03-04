using MediatR;
using Qtemplate.Application.DTOs.RefreshToken;
using Qtemplate.Application.DTOs;
using Qtemplate.Domain.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qtemplate.Application.Features.Admin.Logs.Queries.GetRefreshTokens
{
    public class GetRefreshTokensHandler
      : IRequestHandler<GetRefreshTokensQuery, ApiResponse<PaginatedResult<RefreshTokenDto>>>
    {
        private readonly IRefreshTokenRepository _repo;
        public GetRefreshTokensHandler(IRefreshTokenRepository repo) => _repo = repo;

        public async Task<ApiResponse<PaginatedResult<RefreshTokenDto>>> Handle(
            GetRefreshTokensQuery request, CancellationToken cancellationToken)
        {
            var (items, total) = await _repo.GetPagedAsync(
                request.UserId, request.IsActive, request.Page, request.PageSize);

            return ApiResponse<PaginatedResult<RefreshTokenDto>>.Ok(new PaginatedResult<RefreshTokenDto>
            {
                Items = items.Select(t => new RefreshTokenDto
                {
                    Id = t.Id,
                    UserId = t.UserId,
                    UserEmail = t.User?.Email,
                    Token = t.Token[..Math.Min(20, t.Token.Length)] + "...", // mask
                    IpAddress = t.IpAddress,
                    UserAgent = t.UserAgent,
                    IsRevoked = t.IsRevoked,
                    IsExpired = t.IsExpired,
                    IsActive = t.IsActive,
                    RevokedReason = t.RevokedReason,
                    RevokedAt = t.RevokedAt,
                    ExpiresAt = t.ExpiresAt,
                    CreatedAt = t.CreatedAt
                }).ToList(),
                TotalCount = total,
                Page = request.Page,
                PageSize = request.PageSize
            });
        }
    }
}
