using MediatR;
using Qtemplate.Application.DTOs;
using Qtemplate.Domain.Interfaces.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qtemplate.Application.Features.Admin.IpBlacklist.Commands.DeleteIpBlacklist
{
    public class DeleteIpBlacklistHandler
        : IRequestHandler<DeleteIpBlacklistCommand, ApiResponse<object>>
    {
        private readonly IIpBlacklistRepository _repo;
        public DeleteIpBlacklistHandler(IIpBlacklistRepository repo) => _repo = repo;

        public async Task<ApiResponse<object>> Handle(
            DeleteIpBlacklistCommand request, CancellationToken cancellationToken)
        {
            var entry = await _repo.GetByIdAsync(request.Id);
            if (entry is null)
                return ApiResponse<object>.Fail("Không tìm thấy IP");

            await _repo.DeleteAsync(entry);
            return ApiResponse<object>.Ok(null!, "Đã xóa IP khỏi blacklist");
        }
    }
}
