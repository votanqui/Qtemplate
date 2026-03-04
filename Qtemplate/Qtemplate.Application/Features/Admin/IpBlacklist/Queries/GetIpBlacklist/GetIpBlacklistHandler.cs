using MediatR;
using Qtemplate.Application.DTOs.IpBlacklist;
using Qtemplate.Application.DTOs;
using Qtemplate.Domain.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Qtemplate.Application.Features.Admin.IpBlacklist.Commands.AddIpBlacklist;

namespace Qtemplate.Application.Features.Admin.IpBlacklist.Queries.GetIpBlacklist
{
    public class GetIpBlacklistHandler
      : IRequestHandler<GetIpBlacklistQuery, ApiResponse<PaginatedResult<IpBlacklistDto>>>
    {
        private readonly IIpBlacklistRepository _repo;
        public GetIpBlacklistHandler(IIpBlacklistRepository repo) => _repo = repo;

        public async Task<ApiResponse<PaginatedResult<IpBlacklistDto>>> Handle(
            GetIpBlacklistQuery request, CancellationToken cancellationToken)
        {
            var (items, total) = await _repo.GetPagedAsync(request.Page, request.PageSize);
            return ApiResponse<PaginatedResult<IpBlacklistDto>>.Ok(new PaginatedResult<IpBlacklistDto>
            {
                Items = items.Select(AddIpBlacklistHandler.ToDto).ToList(),
                TotalCount = total,
                Page = request.Page,
                PageSize = request.PageSize
            });
        }
    }
}
